using System;
using Microsoft.AspNet.Identity.EntityFramework;

namespace DrivenIt.Foundation.Infrastructure.Identity
{
    public class User : IdentityUser<Guid, UserLogin, UserRole, UserClaim>
    {
    }
}