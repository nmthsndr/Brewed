using System.ComponentModel.DataAnnotations;

namespace Brewed.DataContext.Entities
{
    public class GuestOrderDetails
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        [Required]
        [StringLength(200)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        // Shipping Address
        [Required]
        [StringLength(200)]
        public string ShippingAddressLine1 { get; set; }

        [StringLength(200)]
        public string? ShippingAddressLine2 { get; set; }

        [Required]
        [StringLength(100)]
        public string ShippingCity { get; set; }

        [Required]
        [StringLength(20)]
        public string ShippingPostalCode { get; set; }

        [Required]
        [StringLength(100)]
        public string ShippingCountry { get; set; }

        [Required]
        [StringLength(20)]
        public string ShippingPhoneNumber { get; set; }

        // Billing Address
        [Required]
        [StringLength(200)]
        public string BillingAddressLine1 { get; set; }

        [StringLength(200)]
        public string? BillingAddressLine2 { get; set; }

        [Required]
        [StringLength(100)]
        public string BillingCity { get; set; }

        [Required]
        [StringLength(20)]
        public string BillingPostalCode { get; set; }

        [Required]
        [StringLength(100)]
        public string BillingCountry { get; set; }

        [Required]
        [StringLength(20)]
        public string BillingPhoneNumber { get; set; }

        public virtual Order Order { get; set; }
    }
}