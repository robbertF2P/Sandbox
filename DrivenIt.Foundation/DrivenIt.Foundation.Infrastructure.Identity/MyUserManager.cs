using System;
using Microsoft.AspNet.Identity;

namespace DrivenIt.Foundation.Infrastructure.Identity
{
    public class MyUserManager : UserManager<User, Guid>
    {
        public MyUserManager(IUserStore<User, Guid> store)
            : base(store)
        {
        }
    }
}