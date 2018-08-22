namespace NServiceBus
{
    using System;
    using Case00038722;
    using MessageMutator;

    public static class HandleIncomingIntegrationTypesConfiguration
    {
        public static void HandleIncomingIntegrationTypes(this EndpointConfiguration configuration, Type[] handledTypes)
        {
            configuration.RegisterMessageMutator(new HandleIncomingIntegrationTypesMutator(handledTypes));
            configuration.AddDeserializer<SystemXmlSerializer>();
        }
    }
}
