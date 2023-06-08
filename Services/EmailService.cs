using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using TheIssueTracker.Models;
using MailKit.Net.Smtp;

namespace TheIssueTracker.Services
{
    public class EmailService : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value; //dependency injection; slightly diff from other injections with other services
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var emailAddress = _emailSettings.EmailAddress ?? Environment.GetEnvironmentVariable("EmailAddress");
                var emailPassword = _emailSettings.EmailPassword ?? Environment.GetEnvironmentVariable("EmailPassword");
                var emailHost = _emailSettings.EmailHost ?? Environment.GetEnvironmentVariable("EmailHost");
                var emailPort = _emailSettings.EmailPort != 0 ? _emailSettings.EmailPort : int.Parse(Environment.GetEnvironmentVariable("EmailPort")!);

                MimeMessage newEmail = new MimeMessage();

                newEmail.Sender = MailboxAddress.Parse(emailAddress);
                foreach (string address in email.Split(";"))
                {
                    newEmail.To.Add(MailboxAddress.Parse(address));
                }

                newEmail.Subject = subject;

                BodyBuilder emailBody = new BodyBuilder(); //creat email using body builder
                emailBody.HtmlBody = htmlMessage; //using html to create body of email

                newEmail.Body = emailBody.ToMessageBody();

                using SmtpClient smtpClient = new SmtpClient();

                try
                {
                    //these 3 lines send out an email
                    await smtpClient.ConnectAsync(emailHost, emailPort, SecureSocketOptions.StartTls);
                    await smtpClient.AuthenticateAsync(emailAddress, emailPassword);
                    await smtpClient.SendAsync(newEmail);

                    //disconnect the client
                    await smtpClient.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    var error = ex.Message;
                    throw;
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
