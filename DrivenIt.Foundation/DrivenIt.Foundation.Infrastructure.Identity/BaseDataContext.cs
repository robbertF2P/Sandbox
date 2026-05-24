using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using Microsoft.AspNet.Identity.EntityFramework;

namespace DrivenIt.Foundation.Infrastructure.Identity
{
    public class BaseDataContext : IdentityDbContext<User, Role, Guid, UserLogin, UserRole, UserClaim>
    {
        public static string Connection { get; set; }

        public BaseDataContext()
            : base((string.IsNullOrEmpty(Connection) ? "name=DataContext" : Connection))
        {
            
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.AutoDetectChangesEnabled = false;
            Configuration.ValidateOnSaveEnabled = false;
            Configuration.ProxyCreationEnabled = false;

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<UserLogin>().HasKey(l => l.UserId);
            modelBuilder.Entity<Role>().HasKey(r => r.Id);
            modelBuilder.Entity<UserRole>().HasKey(r => new { r.RoleId, r.UserId });

            modelBuilder.Configurations.AddFromAssembly(GetType().Assembly);
        }
    }
}