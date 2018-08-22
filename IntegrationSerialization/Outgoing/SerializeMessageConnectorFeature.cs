namespace NServiceBus
{
    using System;
    using Features;
    using MessageInterfaces;
    using Serialization;
    using Settings;
    using Unicast.Messages;

    class SerializeIntegrationTypeFeature : Feature
    {
        const string MainSerializerSettingsKey = "MainSerializer";
        internal const string HandledTypesKey = "SerializeIntegrationTypeFeature.HandledTypes";

        public SerializeIntegrationTypeFeature()
        {
            Prerequisite(
                condition: c =>
                {
                    var settings = c.Settings;
                    return settings.HasExplicitValue(HandledTypesKey);
                },
                description: $"HandledTypes was not present. Use endpointConfiguration.SerializeOutgoingIntegrationTypes()");
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var handledTypes = context.Settings.Get(HandledTypesKey) as Type[];

            if (!context.Settings.TryGet(MainSerializerSettingsKey,
                out Tuple<SerializationDefinition, SettingsHolder> defaultSerializerAndSettings))
            {
                defaultSerializerAndSettings =
                    Tuple.Create<SerializationDefinition, SettingsHolder>(new XmlSerializer(), new SettingsHolder());
            }

            var definition = defaultSerializerAndSettings.Item1;
            var deserializerSettings = defaultSerializerAndSettings.Item2;

            var serializerFactory = definition.Configure(deserializerSettings);

            context.Pipeline.Replace("SerializeMessageConnector", b =>
                {
                    var serializer = serializerFactory(b.Build<IMessageMapper>());
                    return new SerializeIntegrationTypeConnector(serializer, b.Build<MessageMetadataRegistry>(), handledTypes);
                }, "Converts a logical message into a physical message");
        }
    }
}