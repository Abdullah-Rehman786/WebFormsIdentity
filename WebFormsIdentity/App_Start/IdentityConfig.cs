using System;
using System.Configuration;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using SendGrid;
using SendGrid.Helpers.Mail;
using WebFormsIdentity.Models;
using System.Diagnostics;
using System.Net.Mail;
using System.IO;
using System.Web.Hosting;

namespace WebFormsIdentity
{
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            await configSmtpAsync(message);
        }

        private async Task configSmtpAsync(IdentityMessage message)
        {

            /*            var msg = new SendGridMessage()
                        {
                            From = new EmailAddress("abdullah@izazsolutions.com", "WebFormsIdentityApp"),
                            Subject = message.Subject,
                            PlainTextContent = message.Body,
                            HtmlContent = message.Body
                        };
                        msg.AddTo(new EmailAddress(message.Destination, "testCustomer"));

            *//*            var credentials = new NetworkCredential(
                             ConfigurationManager.AppSettings["emailServiceUserName"],
                             ConfigurationManager.AppSettings["emailServicePassword"]
                             );*//*


                        var client = new SendGridClient(ConfigurationManager.AppSettings["SendGridKey"]);
                        msg.SetClickTracking(false, false);
                        var response = await client.SendEmailAsync(msg);*/

            var msg = new MailMessage()
            {
                From = new MailAddress(getSettingsFromFile("emailAddress")),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true

            };
            msg.To.Add(new MailAddress(message.Destination));

            var smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.Credentials = new NetworkCredential(getSettingsFromFile("emailAddress"), getSettingsFromFile("emailPassword"));
            smtpClient.EnableSsl = true;
            smtpClient.Send(msg);
        }

        private string getSettingsFromFile(string key)
        {
            string retrievedSetting = "";
            var objReader = new StreamReader(String.Concat(HostingEnvironment.ApplicationPhysicalPath, "\\PrivateSettings2.txt"));
            do
            {
                var line = objReader.ReadLine();
                if (line.StartsWith(key))
                {
                    retrievedSetting = line.Substring(key.Length+1);
                    break;
                }

            } while (objReader.Peek() != -1);
            return retrievedSetting;
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager) :
            base(userManager, authenticationManager) { }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
