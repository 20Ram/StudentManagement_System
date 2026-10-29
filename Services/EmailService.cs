// using MailKit.Net.Smtp;
// using MimeKit;
// using MailKit.Security;
// namespace CrudOperation.Service
// {
//   public class EmailService : IEmailService
//   {
//     private readonly IConfiguration _configuration;

//     public EmailService(IConfiguration configuration)
//     {
//       _configuration = configuration;
//     }
//     public async Task SendEmailAsync(string toEmail, string subject, string body)
//     {
//       var senderEmail = _configuration["EmailSettings:Email"];
//       var password = _configuration["EmailSettings:Password"];
//       var smtpServer = _configuration["EmailSettings:SmtpServer"];
//       var portValue = _configuration["EmailSettings:Port"];

//       if (!int.TryParse(portValue, out var port) ||
//               port < 1 || port > 65535)
//       {
//         throw new InvalidOperationException(
//             "EmailSettings:Port must be a valid port number.");
//       }

//       var email = new MimeMessage();
//       email.From.Add(
//           new MailboxAddress("Student Management System", senderEmail));
//       email.To.Add(MailboxAddress.Parse(toEmail));
//       email.Subject = subject;
//       email.Body = new TextPart("plain")
//       {
//         Text = body
//       };
//       using var smtp = new SmtpClient();
//       await smtp.ConnectAsync(smtpServer,
//                         int.Parse(portValue),
//                         SecureSocketOptions.StartTls);
//       await smtp.AuthenticateAsync(
//         senderEmail, password 
//       );
//       await smtp.SendAsync(email);
//       await smtp.DisconnectAsync(true);
//     }
//   }
// }



using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CrudOperation.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string body)
        {
            var senderEmail = GetRequiredSetting("EmailSettings:Email");
            var password = GetRequiredSetting("EmailSettings:Password");
            var smtpServer = GetRequiredSetting("EmailSettings:SmtpServer");
            var portValue = GetRequiredSetting("EmailSettings:Port");

            if (!int.TryParse(portValue, out var port) ||
                port < 1 || port > 65535)
            {
                throw new InvalidOperationException(
                    "EmailSettings:Port must be a valid port number.");
            }

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                throw new ArgumentException(
                    "Recipient email cannot be empty.",
                    nameof(toEmail));
            }

            var email = new MimeMessage();
            email.From.Add(
                new MailboxAddress(
                    "Student Management System",
                    senderEmail));

            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject ?? string.Empty;

            email.Body = new TextPart("plain")
            {
                Text = body ?? string.Empty
            };
            
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                smtpServer,
                port,
                SecureSocketOptions.StartTls);

            try
            {
                await smtp.AuthenticateAsync(senderEmail, password);
                await smtp.SendAsync(email);
            }
            finally
            {
                if (smtp.IsConnected)
                {
                    await smtp.DisconnectAsync(true);
                }
            }
        }

        private string GetRequiredSetting(string key)
        {
            var value = _configuration[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    $"Missing or empty configuration setting: {key}");
            }

            return value;
        }
    }
}
