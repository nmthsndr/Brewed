using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brewed.DataContext.Dtos
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public string CouponCode { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string Notes { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public AddressDto ShippingAddress { get; set; }
        public AddressDto BillingAddress { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public InvoiceDto Invoice { get; set; }
        public OrderUserDto User { get; set; }
    }

    public class OrderUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class OrderCreateDto
    {
        [Required]
        public int ShippingAddressId { get; set; }

        public int? BillingAddressId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [StringLength(50)]
        public string? CouponCode { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class OrderStatusUpdateDto
    {
        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class GuestOrderCreateDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public GuestAddressDto ShippingAddress { get; set; }

        [Required]
        public GuestAddressDto BillingAddress { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [StringLength(50)]
        public string? CouponCode { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public string SessionId { get; set; }
    }

    public class GuestAddressDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [StringLength(200)]
        public string AddressLine1 { get; set; }

        [StringLength(200)]
        public string? AddressLine2 { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [StringLength(20)]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; }

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }
    }
}