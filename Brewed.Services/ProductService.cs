using AutoMapper;
using Brewed.DataContext.Context;
using Brewed.DataContext.Dtos;
using Brewed.DataContext.Entities;
using Brewed.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Brewed.Services
{
    public interface IProductService
    {
        Task<PaginatedResultDto<ProductDto>> GetProductsAsync(ProductFilterDto filter);
        Task<ProductDto> GetProductByIdAsync(int productId);
        Task<ProductDto> CreateProductAsync(ProductCreateDto productDto);
        Task<ProductDto> UpdateProductAsync(int productId, ProductUpdateDto productDto);
        Task<bool> DeleteProductAsync(int productId);
    }

    public class ProductService : IProductService
    {
        private readonly BrewedDbContext _context;
        private readonly IMapper _mapper;

        public ProductService(BrewedDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedResultDto<ProductDto>> GetProductsAsync(ProductFilterDto filter)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.Reviews)
                .AsQueryable();

            // Filters
            if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            }

            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(p => p.Name.Contains(filter.Search) || p.Description.Contains(filter.Search));
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);
            }

            if (!string.IsNullOrEmpty(filter.RoastLevel))
            {
                query = query.Where(p => p.RoastLevel == filter.RoastLevel);
            }

            if (!string.IsNullOrEmpty(filter.Origin))
            {
                query = query.Where(p => p.Origin == filter.Origin);
            }

            if (filter.IsCaffeineFree.HasValue)
            {
                query = query.Where(p => p.IsCaffeineFree == filter.IsCaffeineFree.Value);
            }

            if (filter.IsOrganic.HasValue)
            {
                query = query.Where(p => p.IsOrganic == filter.IsOrganic.Value);
            }

            // Sorting
            query = filter.SortBy?.ToLower() switch
            {
                "price-asc" => query.OrderBy(p => p.Price),
                "price-desc" => query.OrderByDescending(p => p.Price),
                "rating" => query.OrderByDescending(p => p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0),
                _ => query.OrderBy(p => p.Name)
            };

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var productDtos = products.Select(p => {
                var imageUrls = !string.IsNullOrEmpty(p.ImageUrl)
                    ? p.ImageUrl.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
                    : new List<string>();

                return new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    RoastLevel = p.RoastLevel,
                    Origin = p.Origin,
                    IsCaffeineFree = p.IsCaffeineFree,
                    IsOrganic = p.IsOrganic,
                    ImageUrl = imageUrls.FirstOrDefault() ?? p.ImageUrl,
                    ImageUrls = imageUrls,
                    CategoryId = p.Category?.Id ?? 0,
                    CategoryName = p.Category?.Name ?? "Unknown",
                    AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = p.Reviews.Count,
                    ProductImages = p.ProductImages.Select(pi => new ProductImageDto
                    {
                        Id = pi.Id,
                        ImageUrl = pi.ImageUrl,
                        DisplayOrder = pi.DisplayOrder
                    }).OrderBy(pi => pi.DisplayOrder).ToList()
                };
            }).ToList();

            return new PaginatedResultDto<ProductDto>
            {
                Items = productDtos,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }

        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            var imageUrls = !string.IsNullOrEmpty(product.ImageUrl)
                ? product.ImageUrl.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>();

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                RoastLevel = product.RoastLevel,
                Origin = product.Origin,
                IsCaffeineFree = product.IsCaffeineFree,
                IsOrganic = product.IsOrganic,
                ImageUrl = imageUrls.FirstOrDefault() ?? product.ImageUrl,
                ImageUrls = imageUrls,
                CategoryId = product.Category?.Id ?? 0,
                CategoryName = product.Category?.Name ?? "Unknown",
                AverageRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = product.Reviews.Count,
                ProductImages = product.ProductImages.Select(pi => new ProductImageDto
                {
                    Id = pi.Id,
                    ImageUrl = pi.ImageUrl,
                    DisplayOrder = pi.DisplayOrder
                }).OrderBy(pi => pi.DisplayOrder).ToList()
            };
        }

        public async Task<ProductDto> CreateProductAsync(ProductCreateDto productDto)
        {
            var category = await _context.Categories.FindAsync(productDto.CategoryId);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found");
            }

            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                StockQuantity = productDto.StockQuantity,
                RoastLevel = productDto.RoastLevel,
                Origin = productDto.Origin,
                IsCaffeineFree = productDto.IsCaffeineFree,
                IsOrganic = productDto.IsOrganic,
                ImageUrl = productDto.ImageUrl,
                Category = category
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return await GetProductByIdAsync(product.Id);
        }

        public async Task<ProductDto> UpdateProductAsync(int productId, ProductUpdateDto productDto)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            var category = await _context.Categories.FindAsync(productDto.CategoryId);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found");
            }

            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Price = productDto.Price;
            product.StockQuantity = productDto.StockQuantity;
            product.RoastLevel = productDto.RoastLevel;
            product.Origin = productDto.Origin;
            product.IsCaffeineFree = productDto.IsCaffeineFree;
            product.IsOrganic = productDto.IsOrganic;
            product.ImageUrl = productDto.ImageUrl;
            product.Category = category;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return await GetProductByIdAsync(product.Id);
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            // Check if product has been ordered (cannot delete products that have been ordered)
            var hasOrderItems = await _context.Set<OrderItem>()
                .AnyAsync(oi => oi.ProductId == productId);

            if (hasOrderItems)
            {
                throw new InvalidOperationException("Cannot delete product that has been ordered. Product is part of existing orders.");
            }

            // Delete related CartItems (it's OK to remove items from carts)
            var cartItems = await _context.Set<CartItem>()
                .Where(ci => ci.ProductId == productId)
                .ToListAsync();

            if (cartItems.Any())
            {
                _context.CartItems.RemoveRange(cartItems);
            }

            // Delete related ProductImages and Reviews
            var productImages = await _context.Set<ProductImage>()
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();

            if (productImages.Any())
            {
                _context.ProductImages.RemoveRange(productImages);
            }

            var reviews = await _context.Set<Review>()
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            if (reviews.Any())
            {
                _context.Reviews.RemoveRange(reviews);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}