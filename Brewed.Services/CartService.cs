using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Brewed.DataContext.Context;
using Brewed.DataContext.Dtos;
using Brewed.DataContext.Entities;

namespace Brewed.Services
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(int? userId, string sessionId);
        Task<CartDto> AddToCartAsync(int? userId, string sessionId, int productId, int quantity);
        Task<CartDto> UpdateCartItemAsync(int cartItemId, int quantity);
        Task<bool> RemoveFromCartAsync(int cartItemId);
        Task<bool> ClearCartAsync(int? userId, string sessionId);
    }

    public class CartService : ICartService
    {
        private readonly BrewedDbContext _context;
        private readonly IMapper _mapper;

        public CartService(BrewedDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private async Task<Cart> GetOrCreateCartAsync(int? userId, string sessionId)
        {
            Cart cart;

            if (userId.HasValue)
            {
                cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId.Value);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.SessionId == sessionId);
            }
            else
            {
                throw new Exception("Either userId or sessionId must be provided");
            }

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    SessionId = sessionId,
                    CartItems = new List<CartItem>()
                };

                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<CartDto> GetCartAsync(int? userId, string sessionId)
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);

            return new CartDto
            {
                Id = cart.Id,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    ProductId = ci.Product.Id,
                    ProductName = ci.Product.Name,
                    ProductImageUrl = ci.Product.ImageUrl,
                    Price = ci.Price,
                    Quantity = ci.Quantity,
                    TotalPrice = ci.Price * ci.Quantity,
                    StockQuantity = ci.Product.StockQuantity
                }).ToList(),
                SubTotal = cart.CartItems.Sum(ci => ci.Price * ci.Quantity),
                TotalItems = cart.CartItems.Sum(ci => ci.Quantity)
            };
        }

        public async Task<CartDto> AddToCartAsync(int? userId, string sessionId, int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            if (product.StockQuantity < quantity)
            {
                throw new Exception("Insufficient stock");
            }

            var cart = await GetOrCreateCartAsync(userId, sessionId);

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;

                if (product.StockQuantity < existingItem.Quantity)
                {
                    throw new Exception("Insufficient stock");
                }

                _context.CartItems.Update(existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    Price = product.Price
                };

                await _context.CartItems.AddAsync(cartItem);
            }

            await _context.SaveChangesAsync();

            return await GetCartAsync(userId, sessionId);
        }

        public async Task<CartDto> UpdateCartItemAsync(int cartItemId, int quantity)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null)
            {
                throw new KeyNotFoundException("Cart item not found");
            }

            if (cartItem.Product.StockQuantity < quantity)
            {
                throw new Exception("Insufficient stock");
            }

            cartItem.Quantity = quantity;
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();

            return await GetCartAsync(cartItem.Cart.UserId, cartItem.Cart.SessionId);
        }

        public async Task<bool> RemoveFromCartAsync(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);

            if (cartItem == null)
            {
                throw new KeyNotFoundException("Cart item not found");
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ClearCartAsync(int? userId, string sessionId)
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);

            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}