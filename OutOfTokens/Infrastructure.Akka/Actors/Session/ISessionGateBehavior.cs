using Akka.Actor;
using System;

namespace Infrastructure.Akka.Actors.Session
{
    public interface ISessionGateBehavior
    {
        TimeSpan IdleTimeout { get; }

        void Attach(ISessionGateHost host);

        Props CreateLoginWorker();

        object CreateLoginRequest();

        void Dispatch(object sessionContext, object work);

        void ReplyWorkFailed(object work, string reason);

        void Logout(object sessionContext);

        bool SessionsEqual(object sessionContext, object sessionToken);
    }
}
