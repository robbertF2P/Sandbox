using System;
using Microsoft.AspNet.Identity.EntityFramework;

namespace DrivenIt.Foundation.Infrastructure.Identity
{
    public class Role : IdentityRole<Guid, UserRole>
    { }
}