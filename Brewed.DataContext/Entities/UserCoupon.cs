using System;
using System.ComponentModel.DataAnnotations;

namespace Brewed.DataContext.Entities
{
    public class UserCoupon
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [Required]
        public int CouponId { get; set; }
        public virtual Coupon Coupon { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UsedDate { get; set; }

        public int? OrderId { get; set; }
        public virtual Order Order { get; set; }
    }
}