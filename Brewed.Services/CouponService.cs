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

        // New methods for user-coupon management
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
            // Generate random code if requested or if code is not provided
            if (couponDto.GenerateRandomCode || string.IsNullOrWhiteSpace(couponDto.Code))
            {
                couponDto.Code = await GenerateRandomCouponCodeAsync();
            }
            else
            {
                // Check if code already exists
                if (await _context.Coupons.AnyAsync(c => c.Code.ToLower() == couponDto.Code.ToLower()))
                {
                    throw new Exception("Coupon code already exists");
                }
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

            // Assign coupon to users if UserIds are provided
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

        // ===== New User-Coupon Management Methods =====

        public async Task<string> GenerateRandomCouponCodeAsync()
        {
            string code;
            bool exists;

            do
            {
                code = CouponCodeGenerator.GenerateFormatted(4, 2); // Generates format like XXXX-XXXX
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
                // Verify user exists
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

                // Send email notifications to newly assigned users
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
                        // Log error but don't fail the assignment
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

            // Get all current assignments
            var existingAssignments = await _context.UserCoupons
                .Where(uc => uc.CouponId == couponId)
                .ToListAsync();

            var existingUserIds = existingAssignments.Select(uc => uc.UserId).ToList();

            // Find users to add (in new list but not in existing)
            var userIdsToAdd = userIds.Except(existingUserIds).ToList();

            // Find users to remove (in existing but not in new list, AND haven't used the coupon)
            var assignmentsToRemove = existingAssignments
                .Where(uc => !userIds.Contains(uc.UserId) && !uc.IsUsed)
                .ToList();

            // Add new users
            if (userIdsToAdd.Any())
            {
                var userCoupons = new List<UserCoupon>();
                foreach (var userId in userIdsToAdd)
                {
                    // Verify user exists
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

            // Remove users who haven't used the coupon
            if (assignmentsToRemove.Any())
            {
                _context.UserCoupons.RemoveRange(assignmentsToRemove);
            }

            await _context.SaveChangesAsync();

            // Send email notifications to newly added users
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
                        // Log error but don't fail the assignment
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

            // User must have the coupon assigned and not used yet
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

            // Check if this coupon has any user assignments
            var hasAssignments = await _context.UserCoupons.AnyAsync(uc => uc.CouponId == coupon.Id);

            // Check if user has this coupon assigned
            var userCoupon = await _context.UserCoupons
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CouponId == coupon.Id);

            // If coupon has assignments, user must have it assigned
            if (hasAssignments && userCoupon == null)
            {
                result.Message = "This coupon is not assigned to you";
                return result;
            }

            // If user has the coupon assigned, check if already used
            if (userCoupon != null && userCoupon.IsUsed)
            {
                result.Message = "You have already used this coupon";
                return result;
            }

            // If coupon is public (no assignments) AND has a usage limit, check if user has used it before via orders
            if (!hasAssignments && coupon.MaxUsageCount.HasValue)
            {
                var hasUsedBefore = await _context.Orders
                    .AnyAsync(o => o.UserId == userId && o.CouponCode.ToLower() == code.ToLower());

                if (hasUsedBefore)
                {
                    result.Message = "You have already used this coupon";
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

            // Check max usage count (global limit for the coupon) - only if set (not unlimited)
            if (coupon.MaxUsageCount.HasValue && coupon.UsageCount >= coupon.MaxUsageCount.Value)
            {
                result.Message = "This coupon has reached its maximum usage limit";
                return result;
            }
            // If MaxUsageCount is null, coupon is unlimited - no check needed

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

            if (userCoupon == null)
            {
                throw new KeyNotFoundException("User coupon assignment not found");
            }

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

        public async Task<List<UserCouponDto>> GetUserCouponsAsync(int userId)
        {
            var userCoupons = await _context.UserCoupons
                .Include(uc => uc.Coupon)
                .Include(uc => uc.Order)
                .Where(uc => uc.UserId == userId)
                .ToListAsync();

            var result = _mapper.Map<List<UserCouponDto>>(userCoupons);

            // Also include public/universal coupons (no user assignments)
            var assignedCouponIds = await _context.UserCoupons
                .Select(uc => uc.CouponId)
                .Distinct()
                .ToListAsync();

            var publicCoupons = await _context.Coupons
                .Where(c => !assignedCouponIds.Contains(c.Id) && c.IsActive)
                .ToListAsync();

            // Check which public coupons the user has already used (via orders)
            var usedPublicCouponCodes = await _context.Orders
                .Where(o => o.UserId == userId && o.CouponCode != null)
                .Select(o => o.CouponCode.ToLower())
                .ToListAsync();

            foreach (var coupon in publicCoupons)
            {
                var isUsed = usedPublicCouponCodes.Contains(coupon.Code.ToLower());

                // If coupon has a max usage count and is used, include it as used
                // If no max usage count (truly unlimited), show as available even if used before
                if (isUsed && !coupon.MaxUsageCount.HasValue)
                {
                    isUsed = false;
                }

                result.Add(new UserCouponDto
                {
                    Id = -coupon.Id, // negative ID to distinguish from real UserCoupon entries
                    UserId = userId,
                    CouponId = coupon.Id,
                    Coupon = _mapper.Map<CouponDto>(coupon),
                    IsUsed = isUsed,
                    AssignedDate = coupon.StartDate,
                    UsedDate = null,
                    OrderId = null
                });
            }

            return result;
        }

        public async Task<List<UserCouponDto>> GetCouponUsersAsync(int couponId)
        {
            var userCoupons = await _context.UserCoupons
                .Include(uc => uc.User)
                .Include(uc => uc.Coupon)
                .Where(uc => uc.CouponId == couponId)
                .ToListAsync();

            return _mapper.Map<List<UserCouponDto>>(userCoupons);
        }
    }
}