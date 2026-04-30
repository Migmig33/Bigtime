using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Net.Mail;
using MailKit.Net.Smtp;
namespace WebApplication1.Services
{
    public class EmailServices
    {
        public static void SendEmail(string toEmail, string code)
        {
            // Implement email sending logic here using SMTP or an email service provider
            // For example, you can use System.Net.Mail.SmtpClient to send emails
            // Make sure to handle exceptions and configure your SMTP settings properly

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Green Residences", ConfigurationManager.AppSettings["SmtpUser"]));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Your Verification Code";

            message.Body = new TextPart("html")
            {
                Text = $@"
                <h2>Email Verification</h2>
                <p>Your verification code is:</p>
                <h1 style='letter-spacing: 8px; color: #4F46E5;'>{code}</h1>
                <p>This code expires in <strong>5 minutes</strong>.</p>
            "

            };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect(
                    ConfigurationManager.AppSettings["SmtpHost"],
                    int.Parse(ConfigurationManager.AppSettings["SmtpPort"]),
                    MailKit.Security.SecureSocketOptions.StartTls
                );
                client.Authenticate(
                    ConfigurationManager.AppSettings["SmtpUser"],
                    ConfigurationManager.AppSettings["SmtpPass"]
                );
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}