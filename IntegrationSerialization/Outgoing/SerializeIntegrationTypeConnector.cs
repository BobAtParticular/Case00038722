namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Logging;
    using Pipeline;
    using Serialization;
    using Unicast.Messages;
    using Serializers.SystemXml;

    class SerializeIntegrationTypeConnector : StageConnector<IOutgoingLogicalMessageContext, IOutgoingPhysicalMessageContext>
    {
        public SerializeIntegrationTypeConnector(IMessageSerializer messageSerializer, MessageMetadataRegistry messageMetadataRegistry, Type[] handledTypes)
        {
            this.messageSerializer = messageSerializer;
            this.messageMetadataRegistry = messageMetadataRegistry;
            this.handledTypes = handledTypes;
            systemXmlMessageSerializer = new SystemXmlMessageSerializer();
        }

        public override async Task Invoke(IOutgoingLogicalMessageContext context, Func<IOutgoingPhysicalMessageContext, Task> stage)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Serializing message '{0}' with id '{1}', ToString() of the message yields: {2}",
                    context.Message.MessageType != null ? context.Message.MessageType.AssemblyQualifiedName : "unknown",
                    context.MessageId, context.Message.Instance);
            }

            if (context.ShouldSkipSerialization())
            {
                await stage(this.CreateOutgoingPhysicalMessageContext(new byte[0], context.RoutingStrategies, context)).ConfigureAwait(false);
                return;
            }

            context.Headers[Headers.ContentType] = messageSerializer.ContentType;
            context.Headers[Headers.EnclosedMessageTypes] =
                SerializeEnclosedMessageTypes(context.Message.MessageType);

            byte[] array;

            if (handledTypes.Contains(context.Message.MessageType))
            {
                using (var stream = new MemoryStream())
                {
                    systemXmlMessageSerializer.Serialize(context.Message.Instance, stream);
                    array = stream.ToArray();
                }
            }
            else
            {
                array = Serialize(context);
            }

            await stage(this.CreateOutgoingPhysicalMessageContext(array, context.RoutingStrategies, context)).ConfigureAwait(false);
        }

        byte[] Serialize(IOutgoingLogicalMessageContext context)
        {
            using (var ms = new MemoryStream())
            {
                messageSerializer.Serialize(context.Message.Instance, ms);
                return ms.ToArray();
            }
        }

        string SerializeEnclosedMessageTypes(Type messageType)
        {
            var metadata = messageMetadataRegistry.GetMessageMetadata(messageType);

            var assemblyQualifiedNames = new List<string>(metadata.MessageHierarchy.Length);
            foreach (var type in metadata.MessageHierarchy)
            {
                var typeAssemblyQualifiedName = type.AssemblyQualifiedName;
                if (assemblyQualifiedNames.Contains(typeAssemblyQualifiedName))
                {
                    continue;
                }

                assemblyQualifiedNames.Add(typeAssemblyQualifiedName);
            }

            return string.Join(";", assemblyQualifiedNames);
        }

        readonly MessageMetadataRegistry messageMetadataRegistry;
        readonly Type[] handledTypes;
        readonly IMessageSerializer messageSerializer;
        readonly IMessageSerializer systemXmlMessageSerializer;

        static readonly ILog log = LogManager.GetLogger<SerializeIntegrationTypeConnector>();
    }
}