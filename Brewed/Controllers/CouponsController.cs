using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Brewed.DataContext.Dtos;
using Brewed.Services;

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
    }
}