
namespace SampleUsage
{
    using NServiceBus;

    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(EndpointConfiguration endpointConfiguration)
        {
            //Configure to use System Xml Serialization for incoming types from non-NserviceBus sources
            endpointConfiguration.HandleIncomingIntegrationTypes(new []
            {
                typeof(PRODTRANS),
                typeof(Transfers)
            });

            //Configure to use System.Xml Serialization for outgoing messages to non-NServiceBus targets
            endpointConfiguration.SerializeOutgoingIntegrationTypes(new []
            {
                typeof(PRODTRANS),
                typeof(Transfers)
            });
        }
    }
}
