using System;
using System.ComponentModel.DataAnnotations;

namespace Brewed.DataContext.Entities
{
    /// Kapcsolótábla a felhasználók és kuponok között.
    /// Tárolja, hogy mely felhasználók kaptak mely kuponokat és felhasználták-e azokat.
    public class UserCoupon
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [Required]
        public int CouponId { get; set; }
        public virtual Coupon Coupon { get; set; }

        /// Jelzi, hogy a felhasználó felhasználta-e már ezt a kupont
        public bool IsUsed { get; set; } = false;

        /// Mikor lett hozzárendelve a kupon a felhasználóhoz
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        /// Mikor lett felhasználva a kupon (ha lett)
        public DateTime? UsedDate { get; set; }

        /// Melyik rendeléshez lett felhasználva (opcionális referencia)
        public int? OrderId { get; set; }
        public virtual Order Order { get; set; }
    }
}