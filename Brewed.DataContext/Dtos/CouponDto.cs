using System.ComponentModel.DataAnnotations;

namespace Brewed.DataContext.Dtos
{
    public class CouponDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int? MaxUsageCount { get; set; }
        public int UsageCount { get; set; }
    }

    public class CouponCreateDto
    {
        [StringLength(50)]
        public string Code { get; set; }  // Optional - can be auto-generated

        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        [RegularExpression("Percentage|FixedAmount")]
        public string DiscountType { get; set; } = "Percentage";

        [Required]
        [Range(0.01, 100000)]
        public decimal DiscountValue { get; set; }

        [Range(0, 10000000)]
        public decimal? MinimumOrderAmount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(1, 1000000)]
        public int? MaxUsageCount { get; set; }

        public List<int> UserIds { get; set; } = new List<int>();  // User IDs to assign the coupon to

        public bool GenerateRandomCode { get; set; } = false;  // Flag to generate random code
    }

    public class CouponValidateDto
    {
        [Required]
        public string Code { get; set; }

        [Required]
        [Range(0.01, 10000000)]
        public decimal OrderAmount { get; set; }
    }

    public class CouponValidationResultDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public decimal DiscountAmount { get; set; }
        public CouponDto Coupon { get; set; }
    }

    public class UserCouponDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public int CouponId { get; set; }
        public CouponDto Coupon { get; set; }
        public bool IsUsed { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? UsedDate { get; set; }
        public int? OrderId { get; set; }
    }

    public class AssignCouponDto
    {
        [Required]
        public int CouponId { get; set; }

        [Required]
        public List<int> UserIds { get; set; } = new List<int>();
    }
}