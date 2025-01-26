using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

namespace Login
{
    public class EmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailService()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            _smtpHost = config["EmailSettings:SmtpHost"];
            _smtpPort = int.Parse(config["EmailSettings:SmtpPort"]);
            _smtpUsername = config["EmailSettings:SmtpUsername"];
            _smtpPassword = config["EmailSettings:SmtpPassword"];
        }

        public void SendVerificationEmail(string email, string verificationCode)
        {
            using (var client = new SmtpClient(_smtpHost, _smtpPort))
            {
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                client.EnableSsl = true;

                var message = new MailMessage
                {
                    From = new MailAddress(email),
                    Subject = "Email Verification Code",
                    Body = $"Your verification code is: {verificationCode}",
                    IsBodyHtml = false
                };

                message.To.Add(email);

                try
                {
                    client.Send(message);
                }
                catch (Exception ex)
                {
                    throw new Exception($"failed to send email: {ex.Message}");
                }
            }
        }
        
    }
}
