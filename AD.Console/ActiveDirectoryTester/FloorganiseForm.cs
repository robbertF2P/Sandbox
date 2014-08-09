using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ActiveDirectoryTester
{
    public partial class FloorganiseForm : Form
    {
        public FloorganiseForm()
        {
            InitializeComponent();
            Console.SetOut(new ListBoxWriter(consoleLb));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            serviceUserTb.Text =  ConfigurationManager.AppSettings["serviceGebruiker"];
            servicePasswordTb.Text = ConfigurationManager.AppSettings["serviceWachtwoord"];
            ldapPortTb.Text = ConfigurationManager.AppSettings["ldapPoort"];
            ldapServerTb.Text = ConfigurationManager.AppSettings["ldapServer"];
            ldapStringTb.Text = ConfigurationManager.AppSettings["ldapString"];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var server = string.Format("{0}:{1}", ldapServerTb.Text, ldapPortTb.Text);
            var path = ldapStringTb.Text;
            var serviceUser = serviceUserTb.Text;
            var servicePassword = servicePasswordTb.Text;
            var username = userTb.Text;
            var password = passwordTb.Text;
            try
            {
                Console.WriteLine("---start---");
                using (
                    var context = new PrincipalContext(ContextType.Domain, server, path, ContextOptions.Negotiate,
                        serviceUser, servicePassword))
                {
                    var isValid = context.ValidateCredentials(username, password, ContextOptions.Negotiate);
                    Console.WriteLine("login was:{0}", isValid);
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception:{0}", ex.Message);
            }
            finally
            {
                Console.WriteLine("---stop---");
            }
        }
    }
}
