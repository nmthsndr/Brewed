using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Brewed.DataContext.Dtos;

namespace Brewed.Services
{
    public interface IPdfService
    {
        byte[] GenerateInvoicePdf(OrderDto order, InvoiceDto invoice);
    }

    public class PdfService : IPdfService
    {
        public PdfService()
        {
            // Configure QuestPDF license (Community license is free for non-commercial use)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateInvoicePdf(OrderDto order, InvoiceDto invoice)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice));
            if (order.Items == null || !order.Items.Any())
                throw new InvalidOperationException("Order must contain at least one item");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .AlignCenter()
                        .Text("INVOICE")
                        .FontSize(28)
                        .SemiBold()
                        .FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            // Invoice details section
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text($"Invoice #: {invoice.InvoiceNumber ?? "N/A"}").SemiBold();
                                    col.Item().Text($"Order #: {order.OrderNumber ?? "N/A"}");
                                    col.Item().Text($"Date: {invoice.IssueDate:yyyy-MM-dd}");
                                    col.Item().Text($"Status: {order.Status ?? "Unknown"}");
                                });

                                row.RelativeItem().AlignRight().Column(col =>
                                {
                                    col.Item().Text("Brewed").FontSize(14).SemiBold();
                                    col.Item().Text("www.brewed.com");
                                });
                            });

                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            // Customer information
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Bill To:").SemiBold().FontSize(12);
                                    col.Item().PaddingTop(5);

                                    if (order.IsGuestOrder && order.GuestOrderDetails != null)
                                    {
                                        var guest = order.GuestOrderDetails;
                                        col.Item().Text($"{guest.FirstName ?? ""} {guest.LastName ?? ""}");
                                        col.Item().Text(guest.Email ?? "");
                                        col.Item().Text(guest.BillingAddressLine1 ?? "");
                                        if (!string.IsNullOrEmpty(guest.BillingAddressLine2))
                                            col.Item().Text(guest.BillingAddressLine2);
                                        col.Item().Text($"{guest.BillingCity ?? ""}, {guest.BillingPostalCode ?? ""}");
                                        col.Item().Text(guest.BillingCountry ?? "");
                                        col.Item().Text(guest.BillingPhoneNumber ?? "");
                                    }
                                    else if (order.BillingAddress != null)
                                    {
                                        var addr = order.BillingAddress;
                                        col.Item().Text($"{addr.FirstName ?? ""} {addr.LastName ?? ""}");
                                        col.Item().Text(order.User?.Email ?? "");
                                        col.Item().Text(addr.AddressLine1 ?? "");
                                        if (!string.IsNullOrEmpty(addr.AddressLine2))
                                            col.Item().Text(addr.AddressLine2);
                                        col.Item().Text($"{addr.City ?? ""}, {addr.PostalCode ?? ""}");
                                        col.Item().Text(addr.Country ?? "");
                                        col.Item().Text(addr.PhoneNumber ?? "");
                                    }
                                    else
                                    {
                                        col.Item().Text("No billing address");
                                    }
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Ship To:").SemiBold().FontSize(12);
                                    col.Item().PaddingTop(5);

                                    if (order.IsGuestOrder && order.GuestOrderDetails != null)
                                    {
                                        var guest = order.GuestOrderDetails;
                                        col.Item().Text($"{guest.FirstName ?? ""} {guest.LastName ?? ""}");
                                        col.Item().Text(guest.ShippingAddressLine1 ?? "");
                                        if (!string.IsNullOrEmpty(guest.ShippingAddressLine2))
                                            col.Item().Text(guest.ShippingAddressLine2);
                                        col.Item().Text($"{guest.ShippingCity ?? ""}, {guest.ShippingPostalCode ?? ""}");
                                        col.Item().Text(guest.ShippingCountry ?? "");
                                        col.Item().Text(guest.ShippingPhoneNumber ?? "");
                                    }
                                    else if (order.ShippingAddress != null)
                                    {
                                        var addr = order.ShippingAddress;
                                        col.Item().Text($"{addr.FirstName ?? ""} {addr.LastName ?? ""}");
                                        col.Item().Text(addr.AddressLine1 ?? "");
                                        if (!string.IsNullOrEmpty(addr.AddressLine2))
                                            col.Item().Text(addr.AddressLine2);
                                        col.Item().Text($"{addr.City ?? ""}, {addr.PostalCode ?? ""}");
                                        col.Item().Text(addr.Country ?? "");
                                        col.Item().Text(addr.PhoneNumber ?? "");
                                    }
                                    else
                                    {
                                        col.Item().Text("No shipping address");
                                    }
                                });
                            });

                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            // Order items table
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);  // Product name
                                    columns.RelativeColumn(1);  // Quantity
                                    columns.RelativeColumn(1);  // Unit Price
                                    columns.RelativeColumn(1);  // Total
                                });

                                // Table header
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Product").SemiBold();
                                    header.Cell().Element(CellStyle).AlignCenter().Text("Qty").SemiBold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Unit Price").SemiBold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Total").SemiBold();
                                });

                                // Table rows
                                foreach (var item in order.Items)
                                {
                                    table.Cell().Element(CellStyle).Text(item.ProductName ?? "Product");
                                    table.Cell().Element(CellStyle).AlignCenter().Text(item.Quantity.ToString());
                                    table.Cell().Element(CellStyle).AlignRight().Text($"€{item.UnitPrice:F2}");
                                    table.Cell().Element(CellStyle).AlignRight().Text($"€{item.TotalPrice:F2}");
                                }

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container
                                        .Border(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Padding(8);
                                }
                            });

                            // Payment information section
                            column.Item().AlignRight().Column(col =>
                            {
                                col.Spacing(5);

                                col.Item().Row(row =>
                                {
                                    row.AutoItem().Width(120).Text("Subtotal:");
                                    row.AutoItem().Text($"€{order.SubTotal:F2}").AlignRight();
                                });

                                col.Item().Row(row =>
                                {
                                    row.AutoItem().Width(120).Text("Shipping:");
                                    row.AutoItem().Text(order.ShippingCost == 0 ? "FREE" : $"€{order.ShippingCost:F2}").AlignRight();
                                });

                                if (order.Discount > 0)
                                {
                                    col.Item().Row(row =>
                                    {
                                        row.AutoItem().Width(120).Text("Discount:");
                                        row.AutoItem().Text($"-€{order.Discount:F2}").FontColor(Colors.Green.Medium).AlignRight();
                                    });

                                    if (!string.IsNullOrEmpty(order.CouponCode))
                                    {
                                        col.Item().Row(row =>
                                        {
                                            row.AutoItem().Width(120).Text("Coupon Code:");
                                            row.AutoItem().Text(order.CouponCode).FontSize(9).AlignRight();
                                        });
                                    }
                                }

                                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                                col.Item().Row(row =>
                                {
                                    row.AutoItem().Width(120).Text("TOTAL:").FontSize(14).SemiBold();
                                    row.AutoItem().Text($"€{order.TotalAmount:F2}").FontSize(14).SemiBold().FontColor(Colors.Blue.Medium).AlignRight();
                                });

                                col.Item().PaddingTop(10).Row(row =>
                                {
                                    row.AutoItem().Width(120).Text("Payment Method:");
                                    row.AutoItem().Text(order.PaymentMethod ?? "Not specified").AlignRight();
                                });
                            });

                            // Notes section
                            if (!string.IsNullOrEmpty(order.Notes))
                            {
                                column.Item().PaddingTop(15).Column(col =>
                                {
                                    col.Item().Text("Notes:").SemiBold();
                                    col.Item().PaddingTop(5).Text(order.Notes).FontSize(10);
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("Thank you for your purchase! | Questions? Contact us at brewedcs@gmail.com")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);
                });
            });

            return document.GeneratePdf();
        }
    }
}