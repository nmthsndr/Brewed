using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Brewed.Services
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string email, string name, string confirmationToken);
        Task SendPasswordResetAsync(string email, string name, string resetToken);
        Task SendOrderConfirmationAsync(string email, string name, string orderNumber, decimal totalAmount);
        Task SendOrderStatusUpdateAsync(string email, string name, string orderNumber, string status);
        Task SendLowStockAlertAsync(string productName, int currentStock);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpHost = _configuration["Email:SmtpHost"];
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
            _smtpUser = _configuration["Email:SmtpUser"];
            _smtpPassword = _configuration["Email:SmtpPassword"];
            _fromEmail = _configuration["Email:FromEmail"];
            _fromName = _configuration["Email:FromName"];
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUser, _smtpPassword),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await client.SendMailAsync(message);
        }

        public async Task SendEmailConfirmationAsync(string email, string name, string verificationCode)
        {
            var subject = "Email megerősítés - Brewed Coffee";

            var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2>Szia {name}!</h2>
                <p>Köszönjük, hogy regisztráltál a Brewed Coffee oldalán!</p>
                <p>Az email címed megerősítéséhez kérjük, add meg az alábbi 6 jegyű kódot:</p>
                <div style='background-color: #f5f5f5; padding: 20px; text-align: center; margin: 20px 0; border-radius: 8px;'>
                    <h1 style='color: #8B4513; font-size: 36px; letter-spacing: 8px; margin: 0;'>{verificationCode}</h1>
                </div>
                <p>Ez a kód 24 óráig érvényes.</p>
                <p>Ha nem te regisztráltál, kérjük, hagyd figyelmen kívül ezt az emailt.</p>
                <p>Üdvözlettel,<br/>Brewed Coffee csapata</p>
            </div>
        ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetAsync(string email, string name, string resetToken)
        {
            var subject = "Jelszó visszaállítás - Brewed Coffee";
            var resetUrl = $"{_configuration["AppUrl"]}/reset-password?token={resetToken}";

            var body = $@"
                <h2>Szia {name}!</h2>
                <p>Kaptunk egy jelszó visszaállítási kérelmet a fiókodhoz.</p>
                <p>Az alábbi linkre kattintva állíthatsz be új jelszót:</p>
                <a href='{resetUrl}' style='background-color: #8B4513; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Új jelszó beállítása</a>
                <p>Ez a link 24 óráig érvényes.</p>
                <p>Ha nem te kérted a jelszó visszaállítást, kérjük, hagyd figyelmen kívül ezt az emailt.</p>
                <p>Üdvözlettel,<br/>Brewed Coffee csapata</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendOrderConfirmationAsync(string email, string name, string orderNumber, decimal totalAmount)
        {
            var subject = $"Rendelés visszaigazolás - {orderNumber}";

            var body = $@"
                <h2>Szia {name}!</h2>
                <p>Köszönjük a rendelésedet!</p>
                <h3>Rendelés részletei:</h3>
                <p><strong>Rendelésszám:</strong> {orderNumber}</p>
                <p><strong>Végösszeg:</strong> {totalAmount:N0} Ft</p>
                <p>A rendelésed feldolgozás alatt van. Hamarosan értesítünk a szállítás állapotáról.</p>
                <p>A rendelésed részleteit megtekintheted a <a href='{_configuration["AppUrl"]}/orders/{orderNumber}'>fiókodban</a>.</p>
                <p>Üdvözlettel,<br/>Brewed Coffee csapata</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendOrderStatusUpdateAsync(string email, string name, string orderNumber, string status)
        {
            var subject = $"Rendelés státusz frissítés - {orderNumber}";

            var statusMessages = new Dictionary<string, string>
            {
                ["Processing"] = "feldolgozás alatt van",
                ["Shipped"] = "feladásra került",
                ["Delivered"] = "kézbesítésre került",
                ["Cancelled"] = "törölve lett"
            };

            var statusText = statusMessages.GetValueOrDefault(status, "frissítve lett");

            var body = $@"
                <h2>Szia {name}!</h2>
                <p>A(z) <strong>{orderNumber}</strong> számú rendelésed {statusText}.</p>
                {(status == "Shipped" ? "<p>A csomag hamarosan megérkezik hozzád!</p>" : "")}
                {(status == "Delivered" ? "<p>Reméljük, elégedett vagy a termékekkel! Kérjük, oszd meg velünk véleményedet!</p>" : "")}
                <p>A rendelésed részleteit megtekintheted a <a href='{_configuration["AppUrl"]}/orders/{orderNumber}'>fiókodban</a>.</p>
                <p>Üdvözlettel,<br/>Brewed Coffee csapata</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendLowStockAlertAsync(string productName, int currentStock)
        {
            var adminEmail = _configuration["Email:AdminEmail"];
            var subject = $"⚠️ Alacsony készlet - {productName}";

            var body = $@"
                <h2>Alacsony készlet riasztás</h2>
                <p><strong>Termék:</strong> {productName}</p>
                <p><strong>Jelenlegi készlet:</strong> {currentStock} db</p>
                <p>Kérjük, ellenőrizd a raktárkészletet és rendeld meg az újratöltést!</p>
            ";

            await SendEmailAsync(adminEmail, subject, body);
        }
    }
}