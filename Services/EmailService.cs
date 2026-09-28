using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
namespace CrudOperation.Service
{
  public class EmailService : IEmailService
  {
    // private readonly AppDbContext _context;
    // public EmailService(AppDbContext context)
    // {
    //   _context = context;
    // }

    private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
      var email = new MimeMessage();
      email.From.Add(
          new MailboxAddress(
              "Student Management System",
              _configuration["EmailSettings:Email"]
              )
          );
          email.To.Add(MailboxAddress.Parse(toEmail));
          email.Subject = subject;
          email.Body = new TextPart("plain")
          {
            Text = body
          };
          using var smtp = new SmtpClient();
          await smtp.ConnectAsync(_configuration["EmailSettings:SmtpServer"],
                            int.Parse(_configuration["EmailSettings:Port"]),
                            SecureSocketOptions.StartTls);
          await smtp.AuthenticateAsync(
            _configuration["EmailSettings:Email"],
            _configuration["EmailSettings:Password"]
          );
          await smtp.SendAsync(email);
          await smtp.DisconnectAsync(true);
    }
  }
}