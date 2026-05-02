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

        Task<string> GenerateRandomCouponCodeAsync();
        Task AssignCouponToUsersAsync(int couponId, List<int> userIds);
        Task UpdateUserAssignmentsAsync(int couponId, List<int> userIds);
        Task<bool> CanUserUseCouponAsync(int userId, string couponCode);
        Task<CouponValidationResultDto> ValidateCouponForUserAsync(int userId, string code, decimal orderAmount);
        Task MarkCouponAsUsedAsync(int userId, string couponCode, int? orderId = null);
        Task<List<UserCouponDto>> GetUserCouponsAsync(int userId);
        Task<List<UserCouponDto>> GetCouponUsersAsync(int couponId);
    }

    public class CouponService : ICouponService
    {
        private readonly BrewedDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public CouponService(BrewedDbContext context, IMapper mapper, IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
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
            if (couponDto.GenerateRandomCode || string.IsNullOrWhiteSpace(couponDto.Code))
            {
                couponDto.Code = await GenerateRandomCouponCodeAsync();
            }
            else
            {
                if (await _context.Coupons.AnyAsync(c => c.Code.ToLower() == couponDto.Code.ToLower()))
                {
                    throw new Exception("Coupon code already exists");
                }
            }

            if (couponDto.EndDate <= couponDto.StartDate)
            {
                throw new Exception("End date must be after start date");
            }

            if (couponDto.DiscountType == "Percentage" && couponDto.DiscountValue > 100)
            {
                throw new Exception("Percentage discount cannot exceed 100%");
            }

            var coupon = _mapper.Map<Coupon>(couponDto);
            coupon.Code = coupon.Code.ToUpper();

            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            if (couponDto.UserIds != null && couponDto.UserIds.Any())
            {
                await AssignCouponToUsersAsync(coupon.Id, couponDto.UserIds);
            }

            return _mapper.Map<CouponDto>(coupon);
        }

        public async Task<CouponDto> UpdateCouponAsync(int couponId, CouponCreateDto couponDto)
        {
            var coupon = await _context.Coupons.FindAsync(couponId);

            if (coupon == null)
            {
                throw new KeyNotFoundException("Coupon not found");
            }

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

            decimal discount = 0;
            if (coupon.DiscountType == "Percentage")
            {
                discount = orderAmount * (coupon.DiscountValue / 100);
            }
            else 
            {
                discount = coupon.DiscountValue;
            }

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

        public async Task<string> GenerateRandomCouponCodeAsync()
        {
            string code;
            bool exists;

            do
            {
                code = CouponCodeGenerator.GenerateFormatted(4, 2);
                exists = await _context.Coupons.AnyAsync(c => c.Code == code);
            } while (exists);

            return code;
        }

        public async Task AssignCouponToUsersAsync(int couponId, List<int> userIds)
        {
            var coupon = await _context.Coupons.FindAsync(couponId);
            if (coupon == null)
            {
                throw new KeyNotFoundException("Coupon not found");
            }

            var existingAssignments = await _context.UserCoupons
                .Where(uc => uc.CouponId == couponId)
                .Select(uc => uc.UserId)
                .ToListAsync();

            var newUserIds = userIds.Except(existingAssignments).ToList();

            var userCoupons = new List<UserCoupon>();
            foreach (var userId in newUserIds)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

                userCoupons.Add(new UserCoupon
                {
                    UserId = userId,
                    CouponId = couponId,
                    IsUsed = false,
                    AssignedDate = DateTime.UtcNow
                });
            }

            if (userCoupons.Any())
            {
                await _context.UserCoupons.AddRangeAsync(userCoupons);
                await _context.SaveChangesAsync();

                var couponDto = _mapper.Map<CouponDto>(coupon);
                var users = await _context.Users
                    .Where(u => newUserIds.Contains(u.Id))
                    .ToListAsync();

                foreach (var user in users)
                {
                    try
                    {
                        await _emailService.SendCouponAssignmentAsync(user.Email, user.Name, couponDto);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send coupon email to {user.Email}: {ex.Message}");
                    }
                }
            }
        }

        public async Task UpdateUserAssignmentsAsync(int couponId, List<int> userIds)
        {
            var coupon = await _context.Coupons.FindAsync(couponId);
            if (coupon == null)
            {
                throw new KeyNotFoundException("Coupon not found");
            }

            var existingAssignments = await _context.UserCoupons
                .Where(uc => uc.CouponId == couponId)
                .ToListAsync();

            var existingUserIds = existingAssignments.Select(uc => uc.UserId).ToList();

            var userIdsToAdd = userIds.Except(existingUserIds).ToList();

            var assignmentsToRemove = existingAssignments
                .Where(uc => !userIds.Contains(uc.UserId) && !uc.IsUsed)
                .ToList();

            if (userIdsToAdd.Any())
            {
                var userCoupons = new List<UserCoupon>();
                foreach (var userId in userIdsToAdd)
                {
                    var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                    if (!userExists)
                    {
                        throw new KeyNotFoundException($"User with ID {userId} not found");
                    }

                    userCoupons.Add(new UserCoupon
                    {
                        UserId = userId,
                        CouponId = couponId,
                        IsUsed = false,
                        AssignedDate = DateTime.UtcNow
                    });
                }

                await _context.UserCoupons.AddRangeAsync(userCoupons);
            }

            if (assignmentsToRemove.Any())
            {
                _context.UserCoupons.RemoveRange(assignmentsToRemove);
            }

            await _context.SaveChangesAsync();

            if (userIdsToAdd.Any())
            {
                var couponDto = _mapper.Map<CouponDto>(coupon);
                var users = await _context.Users
                    .Where(u => userIdsToAdd.Contains(u.Id))
                    .ToListAsync();

                foreach (var user in users)
                {
                    try
                    {
                        await _emailService.SendCouponAssignmentAsync(user.Email, user.Name, couponDto);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send coupon email to {user.Email}: {ex.Message}");
                    }
                }
            }
        }

        public async Task<bool> CanUserUseCouponAsync(int userId, string couponCode)
        {
            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code.ToLower() == couponCode.ToLower());

            if (coupon == null)
            {
                return false;
            }

            var userCoupon = await _context.UserCoupons
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CouponId == coupon.Id);

            return userCoupon != null && !userCoupon.IsUsed;
        }

        public async Task<CouponValidationResultDto> ValidateCouponForUserAsync(int userId, string code, decimal orderAmount)
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

            var hasAssignments = await _context.UserCoupons.AnyAsync(uc => uc.CouponId == coupon.Id);

            var userCoupon = await _context.UserCoupons
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CouponId == coupon.Id);

            if (hasAssignments && userCoupon == null)
            {
                result.Message = "This coupon is not assigned to you";
                return result;
            }

            if (userCoupon != null && userCoupon.IsUsed)
            {
                result.Message = "You have already used this coupon";
                return result;
            }

            if (!hasAssignments && coupon.MaxUsageCount.HasValue)
            {
                var userUsageCount = await _context.Orders
                    .CountAsync(o => o.UserId == userId && o.CouponCode.ToLower() == code.ToLower());

                if (userUsageCount >= coupon.MaxUsageCount.Value)
                {
                    result.Message = "You have reached the maximum usage limit for this coupon";
                    return result;
                }
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

            decimal discount = 0;
            if (coupon.DiscountType == "Percentage")
            {
                discount = orderAmount * (coupon.DiscountValue / 100);
            }
            else 
            {
                discount = coupon.DiscountValue;
            }

            discount = Math.Min(discount, orderAmount);

            result.IsValid = true;
            result.Message = "Coupon applied successfully";
            result.DiscountAmount = discount;
            result.Coupon = _mapper.Map<CouponDto>(coupon);

            return result;
        }

        public async Task MarkCouponAsUsedAsync(int userId, string couponCode, int? orderId = null)
        {
            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code.ToLower() == couponCode.ToLower());

            if (coupon == null)
            {
                throw new KeyNotFoundException("Coupon not found");
            }

            var userCoupon = await _context.UserCoupons
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CouponId == coupon.Id);

            if (userCoupon != null)
            {
                if (userCoupon.IsUsed)
                {
                    throw new Exception("Coupon has already been used by this user");
                }

                userCoupon.IsUsed = true;
                userCoupon.UsedDate = DateTime.UtcNow;
                userCoupon.OrderId = orderId;

                coupon.UsageCount++;

                _context.UserCoupons.Update(userCoupon);
                _context.Coupons.Update(coupon);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<UserCouponDto>> GetUserCouponsAsync(int userId)
        {
            var userCoupons = await _context.UserCoupons
                .Include(uc => uc.Coupon)
                .Include(uc => uc.Order)
                .Where(uc => uc.UserId == userId)
                .ToListAsync();

            var result = _mapper.Map<List<UserCouponDto>>(userCoupons);

            var assignedCouponIds = await _context.UserCoupons
                .Select(uc => uc.CouponId)
                .Distinct()
                .ToListAsync();

            var publicCoupons = await _context.Coupons
                .Where(c => !assignedCouponIds.Contains(c.Id) && c.IsActive)
                .ToListAsync();

            var userOrdersWithCoupons = await _context.Orders
                .Where(o => o.UserId == userId && o.CouponCode != null)
                .Select(o => new { CouponCode = o.CouponCode.ToLower(), o.OrderDate, o.Id })
                .ToListAsync();

            foreach (var coupon in publicCoupons)
            {
                var userCouponOrders = userOrdersWithCoupons
                    .Where(o => o.CouponCode == coupon.Code.ToLower())
                    .ToList();

                var userUsageCount = userCouponOrders.Count;

                var isUsed = coupon.MaxUsageCount.HasValue && userUsageCount >= coupon.MaxUsageCount.Value;

                var lastOrder = userCouponOrders.OrderByDescending(o => o.OrderDate).FirstOrDefault();

                result.Add(new UserCouponDto
                {
                    Id = -coupon.Id,
                    UserId = userId,
                    CouponId = coupon.Id,
                    Coupon = _mapper.Map<CouponDto>(coupon),
                    IsUsed = isUsed,
                    AssignedDate = coupon.StartDate,
                    UsedDate = lastOrder?.OrderDate,
                    OrderId = lastOrder?.Id
                });
            }

            return result;
        }

        public async Task<List<UserCouponDto>> GetCouponUsersAsync(int couponId)
        {
            var userCoupons = await _context.UserCoupons
                .Include(uc => uc.User)
                .Include(uc => uc.Coupon)
                .Where(uc => uc.CouponId == couponId && !uc.User.IsDeleted)
                .ToListAsync();

            return _mapper.Map<List<UserCouponDto>>(userCoupons);
        }
    }
}