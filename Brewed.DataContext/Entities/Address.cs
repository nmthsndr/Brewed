using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brewed.DataContext.Entities
{
    public class Address
    {
        public int Id { get; set; }

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

        public bool IsDefault { get; set; } = false;

        [StringLength(50)]
        public string AddressType { get; set; } = "Shipping"; // Shipping, Billing, Both

        // Nullable for guest addresses
        public int? UserId { get; set; }

        public virtual User User { get; set; }
    }
}