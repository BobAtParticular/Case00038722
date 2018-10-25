using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Extensibility;
using NServiceBus.Features;
using NServiceBus.ObjectBuilder;
using NServiceBus.Pipeline;
using NServiceBus.Unicast.Messages;

public class TransformMessageTypes : Feature
{
    internal const string TransformMessageTypesSettingsKeys = "TransformMessageTypes.Transforms";
    public TransformMessageTypes()
    {
        EnableByDefault();
        Prerequisite(c =>
        {
            var settings = c.Settings;
            return settings.HasExplicitValue(TransformMessageTypesSettingsKeys);
        }, "Transforms have not been defined.");
    }

    protected override void Setup(FeatureConfigurationContext context)
    {
        var transforms = context.Settings.Get(TransformMessageTypesSettingsKeys) as Dictionary<Type, Func<object, object>>;
        context.Pipeline.Register(new TransformMessageTypesBehavior(transforms), "Transforms one message type into another");
    }

    class TransformMessageTypesBehavior : IBehavior<IIncomingLogicalMessageContext,
        IIncomingLogicalMessageContext>
    {
        readonly Dictionary<Type, Func<object, object>> transforms;

        public TransformMessageTypesBehavior(Dictionary<Type, Func<object, object>> transforms)
        {
            this.transforms = transforms;
        }

        public async Task Invoke(IIncomingLogicalMessageContext context,
            Func<IIncomingLogicalMessageContext, Task> next)
        {
            var useContext = context;

            try
            {
                if (transforms.ContainsKey(context.Message.MessageType))
                {
                    var instance = transforms[context.Message.MessageType](context.Message.Instance);
                    useContext = new TransformedIncomingLogicalMessageContext(instance, context);
                }
            }
            finally
            {
                await next(useContext).ConfigureAwait(false);
            }
        }

        class TransformedIncomingLogicalMessageContext : IIncomingLogicalMessageContext
        {
            readonly IIncomingLogicalMessageContext originalContext;

            public TransformedIncomingLogicalMessageContext(object instance,
                IIncomingLogicalMessageContext context)
            {
                Message = new LogicalMessage(new MessageMetadata(instance.GetType()), instance);
                originalContext = context;
            }

            public LogicalMessage Message { get; }

            public Dictionary<string, string> Headers => originalContext.Headers;

            public bool MessageHandled
            {
                get => originalContext.MessageHandled;
                set => originalContext.MessageHandled = value;
            }

            public IBuilder Builder => originalContext.Builder;

            public string MessageId => originalContext.MessageId;

            public string ReplyToAddress => originalContext.ReplyToAddress;

            public IReadOnlyDictionary<string, string> MessageHeaders => originalContext.MessageHeaders;

            public ContextBag Extensions => originalContext.Extensions;

            public Task ForwardCurrentMessageTo(string destination)
            {
                return originalContext.ForwardCurrentMessageTo(destination);
            }

            public Task Publish(object message, PublishOptions options)
            {
                return originalContext.Publish(message, options);
            }

            public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
            {
                return originalContext.Publish<T>(messageConstructor, publishOptions);
            }

            public Task Reply(object message, ReplyOptions options)
            {
                return originalContext.Reply(message, options);
            }

            public Task Reply<T>(Action<T> messageConstructor, ReplyOptions options)
            {
                return originalContext.Reply<T>(messageConstructor, options);
            }

            public Task Send(object message, SendOptions options)
            {
                return originalContext.Send(message, options);
            }

            public Task Send<T>(Action<T> messageConstructor, SendOptions options)
            {
                return originalContext.Send<T>(messageConstructor, options);
            }

            public void UpdateMessageInstance(object newInstance)
            {
                originalContext.UpdateMessageInstance(newInstance);
            }
        }
    }
}
