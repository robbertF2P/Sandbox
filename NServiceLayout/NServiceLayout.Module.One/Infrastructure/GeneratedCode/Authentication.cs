using System;
using NServiceBus;

namespace NServiceLayout.Module.One.Infrastructure
{
    public partial class Authentication : NServiceLayout.Infrastructure.Security.Authentication, IHandleMessages<object>
    {
    }
}
