using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            string server = "sqlserver:389"; // Properly resolves to IP of AD server
            string path = "cn=Users,dc=ad,dc=drivenit,dc=nl";
            string serviceUser = "SerivceUser"; // Username in SAM format - no prefix/suffix
            string servicePassword = "Testtest1";
            string username = "TestUser"; // Username in SAM format - no prefix/suffix
            string password = "Testtest1";



            using (PrincipalContext context = new PrincipalContext(ContextType.Domain, server, path,
                    ContextOptions.Negotiate, serviceUser, servicePassword))
            {
                var isValid = context.ValidateCredentials(username, password, ContextOptions.Negotiate);
                System.Console.WriteLine("login was:{0}", isValid);
            }
            var name = System.Console.ReadLine();
            
        }
    }
}
