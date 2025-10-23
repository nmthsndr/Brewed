using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Brewed.DataContext.Context;
using Brewed.DataContext.Dtos;
using Brewed.DataContext.Entities;

namespace Brewed.Services
{
    public interface IReviewService
    {
        Task<PaginatedResultDto<ReviewDto>> GetProductReviewsAsync(int productId, int page, int pageSize);
        Task<PaginatedResultDto<ReviewDto>> GetAllReviewsAsync(int page, int pageSize);
        Task<ReviewDto> CreateReviewAsync(int userId, ReviewCreateDto reviewDto);
        Task<bool> DeleteReviewAsync(int reviewId, int userId, bool isAdmin = false);
    }

    public class ReviewService : IReviewService
    {
        private readonly BrewedDbContext _context;
        private readonly IMapper _mapper;

        public ReviewService(BrewedDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedResultDto<ReviewDto>> GetProductReviewsAsync(int productId, int page, int pageSize)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();

            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var reviewDtos = reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UserName = r.User.Name,
                UserId = r.UserId,
                ProductId = r.ProductId,
                ProductName = r.Product?.Name
            }).ToList();

            return new PaginatedResultDto<ReviewDto>
            {
                Items = reviewDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<PaginatedResultDto<ReviewDto>> GetAllReviewsAsync(int page, int pageSize)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();

            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var reviewDtos = reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UserName = r.User.Name,
                UserId = r.UserId,
                ProductId = r.ProductId,
                ProductName = r.Product?.Name
            }).ToList();

            return new PaginatedResultDto<ReviewDto>
            {
                Items = reviewDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<ReviewDto> CreateReviewAsync(int userId, ReviewCreateDto reviewDto)
        {
            var product = await _context.Products.FindAsync(reviewDto.ProductId);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            // Check if user already reviewed this product
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == reviewDto.ProductId);

            if (existingReview != null)
            {
                throw new Exception("You have already reviewed this product");
            }

            // Optional: Check if user has purchased this product
            var hasPurchased = await _context.Orders
                .Include(o => o.OrderItems)
                .AnyAsync(o => o.UserId == userId &&
                              o.OrderItems.Any(oi => oi.ProductId == reviewDto.ProductId) &&
                              o.Status == "Delivered");

            if (!hasPurchased)
            {
                throw new Exception("You can only review products you have purchased");
            }

            var review = new Review
            {
                UserId = userId,
                ProductId = reviewDto.ProductId,
                Rating = reviewDto.Rating,
                Title = reviewDto.Title,
                Comment = reviewDto.Comment
            };

            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            return new ReviewDto
            {
                Id = review.Id,
                Rating = review.Rating,
                Title = review.Title,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                UserName = user.Name,
                UserId = userId
            };
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int userId, bool isAdmin = false)
        {
            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null)
            {
                throw new KeyNotFoundException("Review not found");
            }

            if (!isAdmin && review.UserId != userId)
            {
                throw new UnauthorizedAccessException("You don't have permission to delete this review");
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}