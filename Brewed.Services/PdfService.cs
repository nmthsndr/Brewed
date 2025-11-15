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
                                    col.Item().Text($"Invoice #: {invoice.InvoiceNumber}").SemiBold();
                                    col.Item().Text($"Order #: {order.orderNumber}");
                                    col.Item().Text($"Date: {invoice.IssueDate:yyyy-MM-dd}");
                                    col.Item().Text($"Status: {order.status}");
                                });

                                row.RelativeItem().AlignRight().Column(col =>
                                {
                                    col.Item().Text("Brewed Coffee Shop").FontSize(14).SemiBold();
                                    col.Item().Text("Premium Coffee Products");
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

                                    if (order.isGuestOrder && order.guestOrderDetails != null)
                                    {
                                        var guest = order.guestOrderDetails;
                                        col.Item().Text($"{guest.firstName} {guest.lastName}");
                                        col.Item().Text(guest.email);
                                        col.Item().Text(guest.billingAddressLine1);
                                        if (!string.IsNullOrEmpty(guest.billingAddressLine2))
                                            col.Item().Text(guest.billingAddressLine2);
                                        col.Item().Text($"{guest.billingCity}, {guest.billingPostalCode}");
                                        col.Item().Text(guest.billingCountry);
                                        col.Item().Text(guest.billingPhoneNumber);
                                    }
                                    else if (order.billingAddress != null)
                                    {
                                        var addr = order.billingAddress;
                                        col.Item().Text($"{addr.firstName} {addr.lastName}");
                                        col.Item().Text(order.user?.email ?? "");
                                        col.Item().Text(addr.addressLine1);
                                        if (!string.IsNullOrEmpty(addr.addressLine2))
                                            col.Item().Text(addr.addressLine2);
                                        col.Item().Text($"{addr.city}, {addr.postalCode}");
                                        col.Item().Text(addr.country);
                                        col.Item().Text(addr.phoneNumber);
                                    }
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Ship To:").SemiBold().FontSize(12);
                                    col.Item().PaddingTop(5);

                                    if (order.isGuestOrder && order.guestOrderDetails != null)
                                    {
                                        var guest = order.guestOrderDetails;
                                        col.Item().Text($"{guest.firstName} {guest.lastName}");
                                        col.Item().Text(guest.shippingAddressLine1);
                                        if (!string.IsNullOrEmpty(guest.shippingAddressLine2))
                                            col.Item().Text(guest.shippingAddressLine2);
                                        col.Item().Text($"{guest.shippingCity}, {guest.shippingPostalCode}");
                                        col.Item().Text(guest.shippingCountry);
                                        col.Item().Text(guest.shippingPhoneNumber);
                                    }
                                    else if (order.shippingAddress != null)
                                    {
                                        var addr = order.shippingAddress;
                                        col.Item().Text($"{addr.firstName} {addr.lastName}");
                                        col.Item().Text(addr.addressLine1);
                                        if (!string.IsNullOrEmpty(addr.addressLine2))
                                            col.Item().Text(addr.addressLine2);
                                        col.Item().Text($"{addr.city}, {addr.postalCode}");
                                        col.Item().Text(addr.country);
                                        col.Item().Text(addr.phoneNumber);
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
                                foreach (var item in order.orderItems)
                                {
                                    table.Cell().Element(CellStyle).Text(item.product?.name ?? "Product");
                                    table.Cell().Element(CellStyle).AlignCenter().Text(item.quantity.ToString());
                                    table.Cell().Element(CellStyle).AlignRight().Text($"€{item.unitPrice:F2}");
                                    table.Cell().Element(CellStyle).AlignRight().Text($"€{item.totalPrice:F2}");
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
                                    row.AutoItem().Text($"€{order.subTotal:F2}").AlignRight();
                                });

                                col.Item().Row(row =>
                                {
                                    row.AutoItem().Width(120).Text("Shipping:");
                                    row.AutoItem().Text(order.shippingCost == 0 ? "FREE" : $"€{order.shippingCost:F2}").AlignRight();
                                });

                                if (order.discount > 0)
                                {
                                    col.Item().Row(row =>
                                    {
                                        row.AutoItem().Width(120).Text("Discount:");
                                        row.AutoItem().Text($"-€{order.discount:F2}").FontColor(Colors.Green.Medium).AlignRight();
                                    });

                                    if (!string.IsNullOrEmpty(order.couponCode))
                                    {
                                        col.Item().Row(row =>
                                        {
                                            row.AutoItem().Width(120).Text("Coupon Code:");
                                            row.AutoItem().Text(order.couponCode).FontSize(9).AlignRight();
                                        });
                                    }
                                }

                                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                                col.Item().Row(row =>
                                {
                                    row.AutoItem().Width(120).Text("TOTAL:").FontSize(14).SemiBold();
                                    row.AutoItem().Text($"€{order.totalAmount:F2}").FontSize(14).SemiBold().FontColor(Colors.Blue.Medium).AlignRight();
                                });

                                col.Item().PaddingTop(10).Row(row =>
                                {
                                    row.AutoItem().Width(120).Text("Payment Method:");
                                    row.AutoItem().Text(order.paymentMethod).AlignRight();
                                });
                            });

                            // Notes section
                            if (!string.IsNullOrEmpty(order.notes))
                            {
                                column.Item().PaddingTop(15).Column(col =>
                                {
                                    col.Item().Text("Notes:").SemiBold();
                                    col.Item().PaddingTop(5).Text(order.notes).FontSize(10);
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Thank you for your purchase! | ");
                            text.Span("Questions? Contact us at support@brewed.com");
                        })
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);
                });
            });

            return document.GeneratePdf();
        }
    }
}
