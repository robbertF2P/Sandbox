using Akka.Hosting;
using Akka.Hosting.TestKit;
using System;

namespace Infrastructure.Akka.Tests
{
    public abstract class AkkaActorTestKit : TestKit
    {
        protected AkkaActorTestKit(ITestOutputHelper output)
            : base(nameof(AkkaActorTestKit), output)
        {
        }

        protected override void ConfigureAkka(AkkaConfigurationBuilder builder, IServiceProvider provider)
        {
            _ = builder;
            _ = provider;
        }
    }
}
