using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;

namespace DrivenIt.Foundation.Infrastructure.Identity
{
    public class MyUserStore : UserStore<User, Role, Guid, UserLogin, UserRole, UserClaim>,
        IUserStore<User, Guid>
    {
        public MyUserStore(DbContext context)
            : base(context)
        {
        }
    }

}
