namespace Case00038722
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using NServiceBus;
    using NServiceBus.MessageMutator;
    using NServiceBus.Serializers.SystemXml;

    class HandleIncomingIntegrationTypesMutator : IMutateIncomingTransportMessages
    {
        internal const string HandledTypesKey = "HandleIncomingIntegrationTypesMutator.Key";
        private readonly Type[] handledTypes;

        public HandleIncomingIntegrationTypesMutator(Type[] handledTypes)
        {
            this.handledTypes = handledTypes;
        }

        public Task MutateIncoming(MutateIncomingTransportMessageContext context)
        {
            if (!IsMessageBodyEmpty(context.Body) &&
                !context.Headers.ContainsKey(Headers.ContentType) &&
                !context.Headers.ContainsKey(Headers.EnclosedMessageTypes))
            {
                var messageType = DetermineMessageType(handledTypes, Encoding.Default.GetString(context.Body));

                if (messageType != null)
                {
                    context.Headers.Add(Headers.ContentType, SystemXmlSerializer.ContentType);
                    context.Headers.Add(Headers.EnclosedMessageTypes, messageType.AssemblyQualifiedName);
                }
            }

            return Task.CompletedTask;
        }

        private static bool IsMessageBodyEmpty(byte[] body)
        {
            return body == null || body.Length == 0;
        }

        private static Type DetermineMessageType(IEnumerable<Type> possibleTypes, string messageBody)
        {
            foreach (var type in possibleTypes)
            {
                var xmlSerializer = XmlSerializerCache.GetSerializer(type);

                using (var tmpStringReader = new StringReader(messageBody))
                {
                    if (xmlSerializer.CanDeserialize(XmlReader.Create(tmpStringReader)))
                    {
                        return type;
                    }
                }
            }

            // If we got here, we didn't find a matching message type.
            return null;
        }
    }
}
