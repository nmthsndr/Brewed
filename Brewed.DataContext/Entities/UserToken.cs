using System.ComponentModel.DataAnnotations;

namespace Brewed.DataContext.Entities
{
    public class UserToken
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(500)]
        public string Token { get; set; }

        [Required]
        [StringLength(50)]
        public string TokenType { get; set; } // EmailConfirmation, PasswordReset

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public virtual User User { get; set; }
    }
}