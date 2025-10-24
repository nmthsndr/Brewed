using Brewed.DataContext.Dtos;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Brewed.Services
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string email, string name, string confirmationToken);
        Task SendPasswordResetAsync(string email, string name, string resetToken);
        Task SendOrderConfirmationAsync(OrderDto orderDetails);
        Task SendOrderStatusUpdateAsync(string email, string name, string orderNumber, string status);
        Task SendLowStockAlertAsync(string productName, int currentStock);
        Task SendInvoiceEmailAsync(OrderDto orderDetails);
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

        public async Task SendOrderConfirmationAsync(OrderDto orderDetails)
        {
            var subject = $"Order Confirmation - {orderDetails.OrderNumber}";

            // Build product items HTML
            var itemsHtml = string.Join("", orderDetails.Items.Select(item => $@"
                <tr>
                    <td style='padding: 12px; border-bottom: 1px solid #eee;'>
                        <div style='display: flex; align-items: center;'>
                            {(string.IsNullOrEmpty(item.ProductImageUrl) ? "" : $"<img src='{item.ProductImageUrl}' alt='{item.ProductName}' style='width: 50px; height: 50px; object-fit: cover; border-radius: 4px; margin-right: 10px;' />")}
                            <span style='color: #333;'>{item.ProductName}</span>
                        </div>
                    </td>
                    <td style='padding: 12px; border-bottom: 1px solid #eee; text-align: center; color: #666;'>{item.Quantity}</td>
                    <td style='padding: 12px; border-bottom: 1px solid #eee; text-align: right; color: #666;'>€{item.UnitPrice:N2}</td>
                    <td style='padding: 12px; border-bottom: 1px solid #eee; text-align: right; color: #333; font-weight: 600;'>€{item.TotalPrice:N2}</td>
                </tr>
            "));

            // Format addresses
            var shippingAddress = orderDetails.ShippingAddress;
            var billingAddress = orderDetails.BillingAddress ?? orderDetails.ShippingAddress;

            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 700px; margin: 0 auto; background: linear-gradient(135deg, #D4A373 0%, #8B4513 100%); padding: 40px; border-radius: 10px;'>
                    <div style='background: white; padding: 40px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>

                        <!-- Header -->
                        <div style='text-align: center; margin-bottom: 30px; padding-bottom: 20px; border-bottom: 3px solid #8B4513;'>
                            <h1 style='color: #8B4513; margin: 0 0 10px 0; font-size: 32px;'>ORDER CONFIRMATION</h1>
                            <p style='color: #666; margin: 5px 0; font-size: 16px;'><strong>Order Number:</strong> {orderDetails.OrderNumber}</p>
                            <p style='color: #666; margin: 5px 0; font-size: 14px;'>Order Date: {orderDetails.OrderDate:MMMM dd, yyyy}</p>
                        </div>

                        <!-- Customer Info -->
                        <div style='margin-bottom: 30px;'>
                            <h3 style='color: #8B4513; margin-bottom: 10px;'>Hello {orderDetails.User.Name}!</h3>
                            <p style='color: #666; line-height: 1.6;'>Thank you for your order! Your order is being processed and we'll notify you about the shipping status soon.</p>
                        </div>

                        <!-- Order Summary -->
                        <div style='background: #f9f9f9; padding: 15px; border-radius: 6px; margin-bottom: 25px;'>
                            <h3 style='color: #8B4513; margin-top: 0;'>Order Information</h3>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr>
                                    <td style='padding: 8px 0; color: #666;'><strong>Order Status:</strong></td>
                                    <td style='padding: 8px 0; color: #333; text-align: right;'>
                                        <span style='background: #FF9800; color: white; padding: 4px 12px; border-radius: 12px; font-size: 12px;'>{orderDetails.Status}</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #666;'><strong>Payment Method:</strong></td>
                                    <td style='padding: 8px 0; color: #333; text-align: right;'>{orderDetails.PaymentMethod}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #666;'><strong>Payment Status:</strong></td>
                                    <td style='padding: 8px 0; color: #333; text-align: right;'>
                                        <span style='background: {(orderDetails.PaymentStatus == "Paid" ? "#4CAF50" : "#FF9800")}; color: white; padding: 4px 12px; border-radius: 12px; font-size: 12px;'>{orderDetails.PaymentStatus}</span>
                                    </td>
                                </tr>
                            </table>
                        </div>

                        <!-- Product Items -->
                        <div style='margin-bottom: 25px;'>
                            <h3 style='color: #8B4513; margin-bottom: 15px;'>Order Items</h3>
                            <table style='width: 100%; border-collapse: collapse; border: 1px solid #eee; border-radius: 6px; overflow: hidden;'>
                                <thead>
                                    <tr style='background: linear-gradient(135deg, #D4A373 0%, #8B4513 100%);'>
                                        <th style='padding: 12px; text-align: left; color: white; font-weight: 600;'>Product</th>
                                        <th style='padding: 12px; text-align: center; color: white; font-weight: 600;'>Quantity</th>
                                        <th style='padding: 12px; text-align: right; color: white; font-weight: 600;'>Unit Price</th>
                                        <th style='padding: 12px; text-align: right; color: white; font-weight: 600;'>Total</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {itemsHtml}
                                </tbody>
                            </table>
                        </div>

                        <!-- Totals -->
                        <div style='background: #f9f9f9; padding: 20px; border-radius: 6px; margin-bottom: 25px;'>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr>
                                    <td style='padding: 8px 0; color: #666; font-size: 15px;'>Subtotal:</td>
                                    <td style='padding: 8px 0; color: #333; text-align: right; font-size: 15px;'>€{orderDetails.SubTotal:N2}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #666; font-size: 15px;'>Shipping Cost:</td>
                                    <td style='padding: 8px 0; color: #333; text-align: right; font-size: 15px;'>€{orderDetails.ShippingCost:N2}</td>
                                </tr>
                                {(orderDetails.Discount > 0 ? $@"
                                <tr>
                                    <td style='padding: 8px 0; color: #4CAF50; font-size: 15px;'>Discount {(string.IsNullOrEmpty(orderDetails.CouponCode) ? "" : $"({orderDetails.CouponCode})")}:</td>
                                    <td style='padding: 8px 0; color: #4CAF50; text-align: right; font-size: 15px;'>-€{orderDetails.Discount:N2}</td>
                                </tr>
                                " : "")}
                                <tr style='border-top: 2px solid #8B4513;'>
                                    <td style='padding: 15px 0 0 0; color: #8B4513; font-size: 18px; font-weight: bold;'>Total Amount:</td>
                                    <td style='padding: 15px 0 0 0; color: #8B4513; text-align: right; font-size: 20px; font-weight: bold;'>€{orderDetails.TotalAmount:N2}</td>
                                </tr>
                            </table>
                        </div>

                        <!-- Addresses -->
                        <div style='margin-bottom: 25px;'>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr>
                                    <td style='width: 48%; vertical-align: top; padding-right: 2%;'>
                                        <h3 style='color: #8B4513; margin-bottom: 10px; font-size: 16px;'>Shipping Address</h3>
                                        <div style='background: #f9f9f9; padding: 15px; border-radius: 6px; font-size: 14px;'>
                                            <p style='margin: 5px 0; color: #333;'><strong>{shippingAddress.FirstName} {shippingAddress.LastName}</strong></p>
                                            <p style='margin: 5px 0; color: #666;'>{shippingAddress.AddressLine1}</p>
                                            {(string.IsNullOrEmpty(shippingAddress.AddressLine2) ? "" : $"<p style='margin: 5px 0; color: #666;'>{shippingAddress.AddressLine2}</p>")}
                                            <p style='margin: 5px 0; color: #666;'>{shippingAddress.City}, {shippingAddress.PostalCode}</p>
                                            <p style='margin: 5px 0; color: #666;'>{shippingAddress.Country}</p>
                                            <p style='margin: 5px 0; color: #666;'><strong>Phone:</strong> {shippingAddress.PhoneNumber}</p>
                                        </div>
                                    </td>
                                    <td style='width: 48%; vertical-align: top; padding-left: 2%;'>
                                        <h3 style='color: #8B4513; margin-bottom: 10px; font-size: 16px;'>Billing Address</h3>
                                        <div style='background: #f9f9f9; padding: 15px; border-radius: 6px; font-size: 14px;'>
                                            <p style='margin: 5px 0; color: #333;'><strong>{billingAddress.FirstName} {billingAddress.LastName}</strong></p>
                                            <p style='margin: 5px 0; color: #666;'>{billingAddress.AddressLine1}</p>
                                            {(string.IsNullOrEmpty(billingAddress.AddressLine2) ? "" : $"<p style='margin: 5px 0; color: #666;'>{billingAddress.AddressLine2}</p>")}
                                            <p style='margin: 5px 0; color: #666;'>{billingAddress.City}, {billingAddress.PostalCode}</p>
                                            <p style='margin: 5px 0; color: #666;'>{billingAddress.Country}</p>
                                            <p style='margin: 5px 0; color: #666;'><strong>Phone:</strong> {billingAddress.PhoneNumber}</p>
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </div>

                        <!-- Footer -->
                        <div style='text-align: center; padding-top: 20px; border-top: 2px solid #eee;'>
                            <p style='color: #666; font-size: 14px; line-height: 1.6;'>
                                If you have any questions about your order, please contact us at <a href='mailto:{_fromEmail}' style='color: #8B4513;'>{_fromEmail}</a>
                            </p>
                            <p style='color: #333; margin-top: 20px; font-size: 16px;'>Best regards,<br/><span style='color: #8B4513; font-weight: bold;'>Brewed Team</span></p>
                        </div>
                    </div>
                </div>
            ";

            await SendEmailAsync(orderDetails.User.Email, subject, body);
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

        public async Task SendInvoiceEmailAsync(OrderDto orderDetails)
        {
            var subject = $"Invoice {orderDetails.Invoice.InvoiceNumber} - Order {orderDetails.OrderNumber}";

            // Build product items HTML
            var itemsHtml = string.Join("", orderDetails.Items.Select(item => $@"
                <tr>
                    <td style='padding: 12px; border-bottom: 1px solid #eee;'>
                        <div style='display: flex; align-items: center;'>
                            {(string.IsNullOrEmpty(item.ProductImageUrl) ? "" : $"<img src='{item.ProductImageUrl}' alt='{item.ProductName}' style='width: 50px; height: 50px; object-fit: cover; border-radius: 4px; margin-right: 10px;' />")}
                            <span style='color: #333;'>{item.ProductName}</span>
                        </div>
                    </td>
                    <td style='padding: 12px; border-bottom: 1px solid #eee; text-align: center; color: #666;'>{item.Quantity}</td>
                    <td style='padding: 12px; border-bottom: 1px solid #eee; text-align: right; color: #666;'>€{item.UnitPrice:N2}</td>
                    <td style='padding: 12px; border-bottom: 1px solid #eee; text-align: right; color: #333; font-weight: 600;'>€{item.TotalPrice:N2}</td>
                </tr>
            "));

            // Format addresses
            var shippingAddress = orderDetails.ShippingAddress;
            var billingAddress = orderDetails.BillingAddress ?? orderDetails.ShippingAddress;

            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 700px; margin: 0 auto; background: linear-gradient(135deg, #D4A373 0%, #8B4513 100%); padding: 40px; border-radius: 10px;'>
                    <div style='background: white; padding: 40px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>

                        <!-- Header -->
                        <div style='text-align: center; margin-bottom: 30px; padding-bottom: 20px; border-bottom: 3px solid #8B4513;'>
                            <h1 style='color: #8B4513; margin: 0 0 10px 0; font-size: 32px;'>INVOICE</h1>
                            <p style='color: #666; margin: 5px 0; font-size: 16px;'><strong>Invoice Number:</strong> {orderDetails.Invoice.InvoiceNumber}</p>
                            <p style='color: #666; margin: 5px 0; font-size: 14px;'>Issue Date: {orderDetails.Invoice.IssueDate:MMMM dd, yyyy}</p>
                        </div>

                        <!-- Customer Info -->
                        <div style='margin-bottom: 30px;'>
                            <h3 style='color: #8B4513; margin-bottom: 10px;'>Hello {orderDetails.User.Name}!</h3>
                            <p style='color: #666; line-height: 1.6;'>Thank you for your order! Please find your invoice details below.</p>
                        </div>

                        <!-- Order Summary -->
                        <div style='background: #f9f9f9; padding: 15px; border-radius: 6px; margin-bottom: 25px;'>
                            <h3 style='color: #8B4513; margin-top: 0;'>Order Information</h3>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr>
                                    <td style='padding: 8px 0; color: #666;'><strong>Order Number:</strong></td>
                                    <td style='padding: 8px 0; color: #333; text-align: right;'>{orderDetails.OrderNumber}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #666;'><strong>Order Date:</strong></td>
                                    <td style='padding: 8px 0; color: #333; text-align: right;'>{orderDetails.OrderDate:MMMM dd, yyyy}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #666;'><strong>Payment Method:</strong></td>
                                    <td style='padding: 8px 0; color: #333; text-align: right;'>{orderDetails.PaymentMethod}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #666;'><strong>Payment Status:</strong></td>
                                    <td style='padding: 8px 0; color: #333; text-align: right;'>
                                        <span style='background: {(orderDetails.PaymentStatus == "Paid" ? "#4CAF50" : "#FF9800")}; color: white; padding: 4px 12px; border-radius: 12px; font-size: 12px;'>{orderDetails.PaymentStatus}</span>
                                    </td>
                                </tr>
                            </table>
                        </div>

                        <!-- Product Items -->
                        <div style='margin-bottom: 25px;'>
                            <h3 style='color: #8B4513; margin-bottom: 15px;'>Order Items</h3>
                            <table style='width: 100%; border-collapse: collapse; border: 1px solid #eee; border-radius: 6px; overflow: hidden;'>
                                <thead>
                                    <tr style='background: linear-gradient(135deg, #D4A373 0%, #8B4513 100%);'>
                                        <th style='padding: 12px; text-align: left; color: white; font-weight: 600;'>Product</th>
                                        <th style='padding: 12px; text-align: center; color: white; font-weight: 600;'>Quantity</th>
                                        <th style='padding: 12px; text-align: right; color: white; font-weight: 600;'>Unit Price</th>
                                        <th style='padding: 12px; text-align: right; color: white; font-weight: 600;'>Total</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {itemsHtml}
                                </tbody>
                            </table>
                        </div>

                        <!-- Totals -->
                        <div style='background: #f9f9f9; padding: 20px; border-radius: 6px; margin-bottom: 25px;'>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr>
                                    <td style='padding: 8px 0; color: #666; font-size: 15px;'>Subtotal:</td>
                                    <td style='padding: 8px 0; color: #333; text-align: right; font-size: 15px;'>€{orderDetails.SubTotal:N2}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #666; font-size: 15px;'>Shipping Cost:</td>
                                    <td style='padding: 8px 0; color: #333; text-align: right; font-size: 15px;'>€{orderDetails.ShippingCost:N2}</td>
                                </tr>
                                {(orderDetails.Discount > 0 ? $@"
                                <tr>
                                    <td style='padding: 8px 0; color: #4CAF50; font-size: 15px;'>Discount {(string.IsNullOrEmpty(orderDetails.CouponCode) ? "" : $"({orderDetails.CouponCode})")}:</td>
                                    <td style='padding: 8px 0; color: #4CAF50; text-align: right; font-size: 15px;'>-€{orderDetails.Discount:N2}</td>
                                </tr>
                                " : "")}
                                <tr style='border-top: 2px solid #8B4513;'>
                                    <td style='padding: 15px 0 0 0; color: #8B4513; font-size: 18px; font-weight: bold;'>Total Amount:</td>
                                    <td style='padding: 15px 0 0 0; color: #8B4513; text-align: right; font-size: 20px; font-weight: bold;'>€{orderDetails.TotalAmount:N2}</td>
                                </tr>
                            </table>
                        </div>

                        <!-- Addresses -->
                        <div style='margin-bottom: 25px;'>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr>
                                    <td style='width: 48%; vertical-align: top; padding-right: 2%;'>
                                        <h3 style='color: #8B4513; margin-bottom: 10px; font-size: 16px;'>Shipping Address</h3>
                                        <div style='background: #f9f9f9; padding: 15px; border-radius: 6px; font-size: 14px;'>
                                            <p style='margin: 5px 0; color: #333;'><strong>{shippingAddress.FirstName} {shippingAddress.LastName}</strong></p>
                                            <p style='margin: 5px 0; color: #666;'>{shippingAddress.AddressLine1}</p>
                                            {(string.IsNullOrEmpty(shippingAddress.AddressLine2) ? "" : $"<p style='margin: 5px 0; color: #666;'>{shippingAddress.AddressLine2}</p>")}
                                            <p style='margin: 5px 0; color: #666;'>{shippingAddress.City}, {shippingAddress.PostalCode}</p>
                                            <p style='margin: 5px 0; color: #666;'>{shippingAddress.Country}</p>
                                            <p style='margin: 5px 0; color: #666;'><strong>Phone:</strong> {shippingAddress.PhoneNumber}</p>
                                        </div>
                                    </td>
                                    <td style='width: 48%; vertical-align: top; padding-left: 2%;'>
                                        <h3 style='color: #8B4513; margin-bottom: 10px; font-size: 16px;'>Billing Address</h3>
                                        <div style='background: #f9f9f9; padding: 15px; border-radius: 6px; font-size: 14px;'>
                                            <p style='margin: 5px 0; color: #333;'><strong>{billingAddress.FirstName} {billingAddress.LastName}</strong></p>
                                            <p style='margin: 5px 0; color: #666;'>{billingAddress.AddressLine1}</p>
                                            {(string.IsNullOrEmpty(billingAddress.AddressLine2) ? "" : $"<p style='margin: 5px 0; color: #666;'>{billingAddress.AddressLine2}</p>")}
                                            <p style='margin: 5px 0; color: #666;'>{billingAddress.City}, {billingAddress.PostalCode}</p>
                                            <p style='margin: 5px 0; color: #666;'>{billingAddress.Country}</p>
                                            <p style='margin: 5px 0; color: #666;'><strong>Phone:</strong> {billingAddress.PhoneNumber}</p>
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </div>

                        <!-- Footer -->
                        <div style='text-align: center; padding-top: 20px; border-top: 2px solid #eee;'>
                            <p style='color: #666; font-size: 14px; line-height: 1.6;'>
                                If you have any questions about this invoice, please contact us at <a href='mailto:{_fromEmail}' style='color: #8B4513;'>{_fromEmail}</a>
                            </p>
                            <p style='color: #333; margin-top: 20px; font-size: 16px;'>Best regards,<br/><span style='color: #8B4513; font-weight: bold;'>Brewed Team</span></p>
                        </div>
                    </div>
                </div>
            ";

            await SendEmailAsync(orderDetails.User.Email, subject, body);
        }
    }
}