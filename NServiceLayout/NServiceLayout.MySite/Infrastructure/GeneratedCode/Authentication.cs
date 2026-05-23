using System;
using NServiceBus;

namespace NServiceLayout.MySite.Infrastructure
{
    public partial class Authentication : NServiceLayout.Infrastructure.Security.Authentication, IHandleMessages<object>
    {
    }
}
