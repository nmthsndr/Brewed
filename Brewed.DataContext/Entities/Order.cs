using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Brewed.DataContext.Entities
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; }

        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        public string? CouponCode { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Processing";

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ShippingAddressId { get; set; }

        [Required]
        public int BillingAddressId { get; set; }

        // Flag to identify guest orders
        public bool IsGuestOrder { get; set; } = false;

        public virtual User User { get; set; }
        public virtual Address ShippingAddress { get; set; }
        public virtual Address BillingAddress { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual Invoice Invoice { get; set; }
        public virtual GuestOrderDetails GuestOrderDetails { get; set; }
    }
}