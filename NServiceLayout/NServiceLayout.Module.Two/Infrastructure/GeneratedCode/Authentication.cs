using System;
using NServiceBus;

namespace NServiceLayout.Module.Two.Infrastructure
{
    public partial class Authentication : NServiceLayout.Infrastructure.Security.Authentication, IHandleMessages<object>
    {
    }
}
