using System;

namespace DrivenIt.Foundation.Domain.Tools.Events
{
    internal class DomainEventRegistrationRemover : IDisposable
    {
        private readonly Action _callOnDispose;

        public DomainEventRegistrationRemover(Action toCall)
        {
            this._callOnDispose = toCall;
        }

        public void Dispose()
        {
            this._callOnDispose.DynamicInvoke();
        }
    }
}