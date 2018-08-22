using NServiceBus.Configuration.AdvancedExtensibility;

namespace NServiceBus
{
    using System;

    public static class SerializeOutgoingIntegrationTypesConfiguration
    {
        public static void SerializeOutgoingIntegrationTypes(this EndpointConfiguration configuration,
            Type[] handledTypes)
        {
            var settings = configuration.GetSettings();

            settings.Set(SerializeIntegrationTypeFeature.HandledTypesKey, handledTypes);
            configuration.EnableFeature<SerializeIntegrationTypeFeature>();
        }
    }
}
