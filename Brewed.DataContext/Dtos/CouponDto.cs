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
    }

    public class CouponCreateDto
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; }

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
}