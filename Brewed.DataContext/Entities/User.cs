using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Brewed.DataContext.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        // Role: RegisteredUser, Admin
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "RegisteredUser";
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual Cart Cart { get; set; }
        public bool EmailConfirmed { get; set; } = false;
        public virtual ICollection<UserToken> UserTokens { get; set; }
    }
}
