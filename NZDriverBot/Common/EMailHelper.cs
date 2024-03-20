using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace NZDriverBot.Common
{
    public class EMailHelper
    {
        const string SENDER_ADDR = "XXX@gmail.com";
        const string SENDER_PASS = "123456";

        public static Task SentEmailAsync(string email, string subject, string body)
        {
            MailMessage message = new MailMessage();
            message.IsBodyHtml = true;
            message.From = new MailAddress(SENDER_ADDR);
            message.To.Add(email);
            message.Subject = subject;
            message.Body = body;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new System.Net.NetworkCredential(SENDER_ADDR, SENDER_PASS);
            object userState = new object();

            smtpClient.SendCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    Console.WriteLine("Email sent failed：" + e.Error.Message);
                }
                else
                {
                    Console.WriteLine("Email sent successfully！");
                }

                smtpClient.Dispose();
            };

            smtpClient.SendAsync(message, userState);

            return Task.CompletedTask;
        }

    }
}
