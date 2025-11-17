using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Brewed.DataContext.Dtos;
using Brewed.Services;
using System.Security.Claims;

namespace Brewed.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CouponsController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponsController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        // Admin endpoints
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllCoupons()
        {
            try
            {
                var coupons = await _couponService.GetAllCouponsAsync();
                return Ok(coupons);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{couponId}")]
        public async Task<IActionResult> GetCoupon(int couponId)
        {
            try
            {
                var coupon = await _couponService.GetCouponByIdAsync(couponId);
                return Ok(coupon);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Coupon not found");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateCoupon([FromBody] CouponCreateDto couponDto)
        {
            try
            {
                var result = await _couponService.CreateCouponAsync(couponDto);
                return CreatedAtAction(nameof(GetCoupon), new { couponId = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{couponId}")]
        public async Task<IActionResult> UpdateCoupon(int couponId, [FromBody] CouponCreateDto couponDto)
        {
            try
            {
                var result = await _couponService.UpdateCouponAsync(couponId, couponDto);

                // Update user assignments if UserIds are provided
                if (couponDto.UserIds != null && couponDto.UserIds.Any())
                {
                    await _couponService.UpdateUserAssignmentsAsync(couponId, couponDto.UserIds);
                }

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{couponId}")]
        public async Task<IActionResult> DeleteCoupon(int couponId)
        {
            try
            {
                var result = await _couponService.DeleteCouponAsync(couponId);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Coupon not found");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Public endpoint for validation
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateCoupon([FromBody] CouponValidateDto dto)
        {
            try
            {
                var result = await _couponService.ValidateCouponAsync(dto.Code, dto.OrderAmount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// Generate a random coupon code
        [Authorize(Roles = "Admin")]
        [HttpGet("generate-code")]
        public async Task<IActionResult> GenerateRandomCouponCode()
        {
            try
            {
                var code = await _couponService.GenerateRandomCouponCodeAsync();
                return Ok(new { code });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// Assign coupon to users
        [Authorize(Roles = "Admin")]
        [HttpPost("assign")]
        public async Task<IActionResult> AssignCouponToUsers([FromBody] AssignCouponDto dto)
        {
            try
            {
                await _couponService.AssignCouponToUsersAsync(dto.CouponId, dto.UserIds);
                return Ok(new { message = "Coupon assigned successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// Get all coupons for a specific user
        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserCoupons(int userId)
        {
            try
            {
                // Users can only see their own coupons unless they're admin
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Admin");

                if (!isAdmin && currentUserId != userId.ToString())
                {
                    return Forbid();
                }

                var userCoupons = await _couponService.GetUserCouponsAsync(userId);
                return Ok(userCoupons);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// Get all users assigned to a specific coupon (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet("{couponId}/users")]
        public async Task<IActionResult> GetCouponUsers(int couponId)
        {
            try
            {
                var couponUsers = await _couponService.GetCouponUsersAsync(couponId);
                return Ok(couponUsers);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// Validate coupon for a specific user (checks if user has the coupon and hasn't used it)
        [Authorize]
        [HttpPost("validate-for-user")]
        public async Task<IActionResult> ValidateCouponForUser([FromBody] CouponValidateDto dto)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                var result = await _couponService.ValidateCouponForUserAsync(userId, dto.Code, dto.OrderAmount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// Check if user can use a specific coupon
        [Authorize]
        [HttpGet("can-use/{couponCode}")]
        public async Task<IActionResult> CanUserUseCoupon(string couponCode)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                var canUse = await _couponService.CanUserUseCouponAsync(userId, couponCode);
                return Ok(new { canUse });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}