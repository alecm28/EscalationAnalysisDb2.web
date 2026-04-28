using System.Net;
using System.Net.Mail;

namespace EscalationAnalysisDb2.Application.Services
{
    public class EmailService
    {
        public async Task SendEmail(string toEmail, string subject, string resetLink)
        {
            // correo que envia mensajes
            var fromEmail = "escalationanalysis@gmail.com";

            // clave de aplicacion de gmail
            //var password = "kjcldbbrjkhzgpyo";
            var password = "wmywlpvlqfcjpcdj";

            // configuracion smtp
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true,
            };

            // cuerpo html del correo
            var body = $@"
<div style='font-family:Segoe UI, Arial; background:#f3f4f6; padding:20px;'>

    <div style='max-width:500px; margin:auto; background:white; border-radius:12px; padding:30px;'>

        <h2 style='color:#4c1d95; text-align:center; margin-bottom:10px;'>
            Escalation Analysis
        </h2>

        <h3 style='text-align:center; margin-bottom:20px;'>
            Reset your password
        </h3>

        <p style='color:#555; text-align:center;'>
            We received a request to reset your password.
        </p>

        <div style='text-align:center; margin:30px 0;'>
            <a href='{resetLink}' 
               style='background:#4c1d95; color:white; padding:12px 25px; border-radius:8px; text-decoration:none; font-weight:500;'>
                Reset Password
            </a>
        </div>

        <p style='color:#999; font-size:12px; text-align:center;'>
            This link will expire in 1 hour.
        </p>

        <p style='color:#999; font-size:12px; text-align:center;'>
            If you didn’t request this, ignore this email.
        </p>

    </div>

</div>
";

            // mensaje final que se envia
            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, "Escalation Analysis - Password Reset"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            // destinatario
            message.To.Add(toEmail);

            // envio del correo
            await smtpClient.SendMailAsync(message);
        }
    }
}