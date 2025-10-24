using System.ComponentModel.DataAnnotations;

namespace Brewed.DataContext.Dtos
{
    public class GuestOrderDetailsDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // Shipping Address
        public string ShippingAddressLine1 { get; set; }
        public string? ShippingAddressLine2 { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingPostalCode { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingPhoneNumber { get; set; }

        // Billing Address
        public string BillingAddressLine1 { get; set; }
        public string? BillingAddressLine2 { get; set; }
        public string BillingCity { get; set; }
        public string BillingPostalCode { get; set; }
        public string BillingCountry { get; set; }
        public string BillingPhoneNumber { get; set; }
    }
}