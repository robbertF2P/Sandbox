using Floor2Plan.TestUtility.Common.Akka;

namespace Infrastructure.Akka.Tests
{
    public abstract class AkkaActorTestKit : AkkaSerilogTestKit
    {
        protected AkkaActorTestKit(ITestOutputHelper output)
            : base(nameof(AkkaActorTestKit), output)
        {
        }
    }
}
