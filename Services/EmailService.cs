using System.Net.Mail;
using System.Net;

namespace Auth.Services
{
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _port;
        private readonly string _emailFrom;
        private readonly string _emailPassword;
        private readonly bool _enableSsl;

        public EmailService(IConfiguration configuration)
        {
            _smtpServer = configuration["EmailSettings:SmtpServer"];
            _port = int.Parse(configuration["EmailSettings:Port"]);
            _emailFrom = configuration["EmailSettings:FromEmail"];
            _emailPassword = configuration["EmailSettings:EmailPassword"];
            _enableSsl = bool.Parse(configuration["EmailSettings:EnableSsl"]);
        }

        public void SendOtp(string toEmail, string otp)
        {
            try
            {
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress(_emailFrom),
                    Subject = "Your OTP Code",
                    Body = $"Your OTP code is: {otp}",
                    IsBodyHtml = true
                };

                mail.To.Add(toEmail);

                SmtpClient smtpClient = new SmtpClient(_smtpServer)
                {
                    Port = _port,
                    Credentials = new NetworkCredential(_emailFrom, _emailPassword),
                    EnableSsl = _enableSsl
                };

                smtpClient.Send(mail);
            }
            catch (SmtpException smtpEx)
            {
                throw new Exception("SMTP Error: " + smtpEx.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Email Error: " + ex.Message);
            }
        }
    }
}
