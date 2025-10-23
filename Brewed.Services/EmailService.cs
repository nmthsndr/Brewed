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
            var subject = "Email Confirmation - Brewed";

            var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background: linear-gradient(135deg, #D4A373 0%, #8B4513 100%); padding: 40px; border-radius: 10px;'>
                <div style='background: white; padding: 30px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                    <h2 style='color: #8B4513; margin-bottom: 20px;'>Hello {name}!</h2>
                    <p style='color: #333; font-size: 16px; line-height: 1.6;'>Thank you for registering at Brewed!</p>
                    <p style='color: #333; font-size: 16px; line-height: 1.6;'>To confirm your email address, please enter the following 6-digit code:</p>
                    <div style='background: linear-gradient(135deg, #D4A373 0%, #8B4513 100%); padding: 25px; text-align: center; margin: 30px 0; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                        <h1 style='color: white; font-size: 42px; letter-spacing: 10px; margin: 0; text-shadow: 2px 2px 4px rgba(0,0,0,0.2);'>{verificationCode}</h1>
                    </div>
                    <p style='color: #666; font-size: 14px;'>This code is valid for 24 hours.</p>
                    <p style='color: #666; font-size: 14px;'>If you didn't register, please ignore this email.</p>
                    <p style='color: #333; font-size: 16px; margin-top: 30px;'>Best regards,<br/><span style='color: #8B4513; font-weight: bold;'>Brewed Team</span></p>
                </div>
            </div>
        ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetAsync(string email, string name, string resetCode)
        {
            var subject = "Password Reset - Brewed";

            var body = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background: linear-gradient(135deg, #D4A373 0%, #8B4513 100%); padding: 40px; border-radius: 10px;'>
            <div style='background: white; padding: 30px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                <h2 style='color: #8B4513; margin-bottom: 20px;'>Hello {name}!</h2>
                <p style='color: #333; font-size: 16px; line-height: 1.6;'>We received a password reset request for your account.</p>
                <p style='color: #333; font-size: 16px; line-height: 1.6;'>To reset your password, please enter the following 6-digit code:</p>
                <div style='background: linear-gradient(135deg, #D4A373 0%, #8B4513 100%); padding: 25px; text-align: center; margin: 30px 0; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                    <h1 style='color: white; font-size: 42px; letter-spacing: 10px; margin: 0; text-shadow: 2px 2px 4px rgba(0,0,0,0.2);'>{resetCode}</h1>
                </div>
                <p style='color: #666; font-size: 14px;'>This code is valid for 1 hour.</p>
                <p style='color: #666; font-size: 14px;'>If you didn't request a password reset, please ignore this email and your password will remain unchanged.</p>
                <p style='color: #333; font-size: 16px; margin-top: 30px;'>Best regards,<br/><span style='color: #8B4513; font-weight: bold;'>Brewed Team</span></p>
            </div>
        </div>
    ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendOrderConfirmationAsync(string email, string name, string orderNumber, decimal totalAmount)
        {
            var subject = $"Order Confirmation - {orderNumber}";

            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background: linear-gradient(135deg, #D4A373 0%, #8B4513 100%); padding: 40px; border-radius: 10px;'>
                    <div style='background: white; padding: 30px; border-radius: 8px;'>
                        <h2 style='color: #8B4513;'>Hello {name}!</h2>
                        <p>Thank you for your order!</p>
                        <h3 style='color: #8B4513;'>Order Details:</h3>
                        <p><strong>Order Number:</strong> {orderNumber}</p>
                        <p><strong>Total Amount:</strong> €{totalAmount:N2}</p>
                        <p>Your order is being processed. We'll notify you about the shipping status soon.</p>
                        <p>You can view your order details in your <a href='{_configuration["AppUrl"]}/orders/{orderNumber}' style='color: #8B4513;'>account</a>.</p>
                        <p style='color: #333; margin-top: 30px;'>Best regards,<br/><span style='color: #8B4513; font-weight: bold;'>Brewed Team</span></p>
                    </div>
                </div>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendOrderStatusUpdateAsync(string email, string name, string orderNumber, string status)
        {
            var subject = $"Order Status Update - {orderNumber}";

            var statusMessages = new Dictionary<string, string>
            {
                ["Processing"] = "is being processed",
                ["Shipped"] = "has been shipped",
                ["Delivered"] = "has been delivered",
                ["Cancelled"] = "has been cancelled"
            };

            var statusText = statusMessages.GetValueOrDefault(status, "has been updated");

            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background: linear-gradient(135deg, #D4A373 0%, #8B4513 100%); padding: 40px; border-radius: 10px;'>
                    <div style='background: white; padding: 30px; border-radius: 8px;'>
                        <h2 style='color: #8B4513;'>Hello {name}!</h2>
                        <p>Your order <strong>{orderNumber}</strong> {statusText}.</p>
                        {(status == "Shipped" ? "<p>Your package will arrive soon!</p>" : "")}
                        {(status == "Delivered" ? "<p>We hope you're satisfied with your products! Please share your feedback with us!</p>" : "")}
                        <p>You can view your order details in your account.</p>
                        <p style='color: #333; margin-top: 30px;'>Best regards,<br/><span style='color: #8B4513; font-weight: bold;'>Brewed Team</span></p>
                    </div>
                </div>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendLowStockAlertAsync(string productName, int currentStock)
        {
            var adminEmail = _configuration["Email:AdminEmail"];
            var subject = $"Low Stock Alert - {productName}";

            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #8B4513;'>Low Stock Alert</h2>
                    <p><strong>Product:</strong> {productName}</p>
                    <p><strong>Current Stock:</strong> {currentStock} pcs</p>
                    <p>Please check the inventory and order a restock!</p>
                </div>
            ";

            await SendEmailAsync(adminEmail, subject, body);
        }
    }
}