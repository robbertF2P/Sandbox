namespace NSignaler.Processor 
{
    using NServiceBus;

	/*
		This class configures this endpoint as a Server. More information about how to configure the NServiceBus host
		can be found here: http://particular.net/articles/profiles-for-nservicebus-host
	*/
    public class EndpointConfig : IConfigureThisEndpoint, IWantCustomInitialization, AsA_Server
    {
        public void Init()
        {
            Configure.With()
                     .DefineEndpointName("NSignaler.Processor")
                     .DefaultBuilder()
                     .JsonSerializer()
                     .MsmqTransport()
                     .UnicastBus()
                     .MsmqSubscriptionStorage()
                     .UseInMemoryTimeoutPersister();
        }
    }
}