using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Brewed.DataContext.Context;
using Brewed.DataContext.Dtos;
using Brewed.DataContext.Entities;

namespace Brewed.Services
{
    public interface ICouponService
    {
        Task<List<CouponDto>> GetAllCouponsAsync();
        Task<CouponDto> GetCouponByIdAsync(int couponId);
        Task<CouponDto> GetCouponByCodeAsync(string code);
        Task<CouponDto> CreateCouponAsync(CouponCreateDto couponDto);
        Task<CouponDto> UpdateCouponAsync(int couponId, CouponCreateDto couponDto);
        Task<bool> DeleteCouponAsync(int couponId);
        Task<CouponValidationResultDto> ValidateCouponAsync(string code, decimal orderAmount);
        Task<decimal> ApplyCouponAsync(string code, decimal orderAmount);
    }

    public class CouponService : ICouponService
    {
        private readonly BrewedDbContext _context;
        private readonly IMapper _mapper;

        public CouponService(BrewedDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<CouponDto>> GetAllCouponsAsync()
        {
            var coupons = await _context.Coupons.ToListAsync();
            return _mapper.Map<List<CouponDto>>(coupons);
        }

        public async Task<CouponDto> GetCouponByIdAsync(int couponId)
        {
            var coupon = await _context.Coupons.FindAsync(couponId);

            if (coupon == null)
            {
                throw new KeyNotFoundException("Coupon not found");
            }

            return _mapper.Map<CouponDto>(coupon);
        }

        public async Task<CouponDto> GetCouponByCodeAsync(string code)
        {
            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower());

            if (coupon == null)
            {
                throw new KeyNotFoundException("Coupon not found");
            }

            return _mapper.Map<CouponDto>(coupon);
        }

        public async Task<CouponDto> CreateCouponAsync(CouponCreateDto couponDto)
        {
            // Check if code already exists
            if (await _context.Coupons.AnyAsync(c => c.Code.ToLower() == couponDto.Code.ToLower()))
            {
                throw new Exception("Coupon code already exists");
            }

            // Validate dates
            if (couponDto.EndDate <= couponDto.StartDate)
            {
                throw new Exception("End date must be after start date");
            }

            // Validate discount value
            if (couponDto.DiscountType == "Percentage" && couponDto.DiscountValue > 100)
            {
                throw new Exception("Percentage discount cannot exceed 100%");
            }

            var coupon = _mapper.Map<Coupon>(couponDto);
            coupon.Code = coupon.Code.ToUpper();

            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            return _mapper.Map<CouponDto>(coupon);
        }

        public async Task<CouponDto> UpdateCouponAsync(int couponId, CouponCreateDto couponDto)
        {
            var coupon = await _context.Coupons.FindAsync(couponId);

            if (coupon == null)
            {
                throw new KeyNotFoundException("Coupon not found");
            }

            // Check if new code conflicts with existing (excluding current)
            if (await _context.Coupons.AnyAsync(c =>
                c.Code.ToLower() == couponDto.Code.ToLower() && c.Id != couponId))
            {
                throw new Exception("Coupon code already exists");
            }

            if (couponDto.EndDate <= couponDto.StartDate)
            {
                throw new Exception("End date must be after start date");
            }

            if (couponDto.DiscountType == "Percentage" && couponDto.DiscountValue > 100)
            {
                throw new Exception("Percentage discount cannot exceed 100%");
            }

            _mapper.Map(couponDto, coupon);
            coupon.Code = coupon.Code.ToUpper();

            _context.Coupons.Update(coupon);
            await _context.SaveChangesAsync();

            return _mapper.Map<CouponDto>(coupon);
        }

        public async Task<bool> DeleteCouponAsync(int couponId)
        {
            var coupon = await _context.Coupons.FindAsync(couponId);

            if (coupon == null)
            {
                throw new KeyNotFoundException("Coupon not found");
            }

            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<CouponValidationResultDto> ValidateCouponAsync(string code, decimal orderAmount)
        {
            var result = new CouponValidationResultDto
            {
                IsValid = false,
                Message = "Invalid coupon",
                DiscountAmount = 0
            };

            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower());

            if (coupon == null)
            {
                result.Message = "Coupon code not found";
                return result;
            }

            if (!coupon.IsActive)
            {
                result.Message = "This coupon is no longer active";
                return result;
            }

            var now = DateTime.UtcNow;
            if (now < coupon.StartDate)
            {
                result.Message = $"This coupon is valid from {coupon.StartDate:yyyy.MM.dd}";
                return result;
            }

            if (now > coupon.EndDate)
            {
                result.Message = "This coupon has expired";
                return result;
            }

            if (coupon.MinimumOrderAmount.HasValue && orderAmount < coupon.MinimumOrderAmount.Value)
            {
                result.Message = $"Minimum order amount is €{coupon.MinimumOrderAmount.Value:N2}";
                return result;
            }

            // Calculate discount
            decimal discount = 0;
            if (coupon.DiscountType == "Percentage")
            {
                discount = orderAmount * (coupon.DiscountValue / 100);
            }
            else // FixedAmount
            {
                discount = coupon.DiscountValue;
            }

            // Discount cannot exceed order amount
            discount = Math.Min(discount, orderAmount);

            result.IsValid = true;
            result.Message = "Coupon applied successfully";
            result.DiscountAmount = discount;
            result.Coupon = _mapper.Map<CouponDto>(coupon);

            return result;
        }

        public async Task<decimal> ApplyCouponAsync(string code, decimal orderAmount)
        {
            var validation = await ValidateCouponAsync(code, orderAmount);

            if (!validation.IsValid)
            {
                throw new Exception(validation.Message);
            }

            return validation.DiscountAmount;
        }
    }
}