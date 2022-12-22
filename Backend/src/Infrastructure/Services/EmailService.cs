namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly string _address;
    private readonly string _password;

    public EmailService(IConfiguration configuration)
    {
        _address = configuration.GetSection("Email:Address").Value;
        _password = configuration.GetSection("Email:Password").Value;
    }

    public void SendMail(string userEmail, string subject, string message)
    {
        var email = new MimeMessage();
        
        email.From.Add(new MailboxAddress("Twitter", _address));
        
        email.To.Add(MailboxAddress.Parse(userEmail));
        
        email.Subject = subject;
        
        email.Body = new TextPart(TextFormat.Html) { Text = message };

        var smtp = new SmtpClient();

        smtp.Connect("smtp.gmail.com", 465, true);
       
        smtp.Authenticate(_address, _password);
        
        smtp.Send(email);
        
        smtp.Disconnect(true);
    }
}
