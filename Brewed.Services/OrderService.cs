using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Brewed.DataContext.Context;
using Brewed.DataContext.Dtos;
using Brewed.DataContext.Entities;

namespace Brewed.Services
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetUserOrdersAsync(int userId);
        Task<OrderDto> GetOrderByIdAsync(int orderId, int userId, bool isAdmin = false);
        Task<OrderDto> CreateOrderAsync(int userId, OrderCreateDto orderDto);
        Task<OrderDto> CancelOrderAsync(int orderId, int userId);
        Task<PaginatedResultDto<OrderDto>> GetAllOrdersAsync(string status, int page, int pageSize);
        Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatusUpdateDto statusDto);
        Task<InvoiceDto> GetInvoiceAsync(int orderId, int userId, bool isAdmin = false);
    }

    public class OrderService : IOrderService
    {
        private readonly BrewedDbContext _context;
        private readonly IMapper _mapper;

        public OrderService(BrewedDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<OrderDto>> GetUserOrdersAsync(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.Invoice)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(MapOrderToDto).ToList();
        }

        public async Task<OrderDto> GetOrderByIdAsync(int orderId, int userId, bool isAdmin = false)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.Invoice)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            if (!isAdmin && order.UserId != userId)
            {
                throw new UnauthorizedAccessException("You don't have permission to view this order");
            }

            return MapOrderToDto(order);
        }

        public async Task<OrderDto> CreateOrderAsync(int userId, OrderCreateDto orderDto)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                throw new Exception("Cart is empty");
            }

            var shippingAddress = await _context.Addresses.FindAsync(orderDto.ShippingAddressId);
            if (shippingAddress == null || shippingAddress.UserId != userId)
            {
                throw new KeyNotFoundException("Shipping address not found");
            }

            Address billingAddress = null;
            if (orderDto.BillingAddressId.HasValue)
            {
                billingAddress = await _context.Addresses.FindAsync(orderDto.BillingAddressId.Value);
                if (billingAddress == null || billingAddress.UserId != userId)
                {
                    throw new KeyNotFoundException("Billing address not found");
                }
            }

            // Check stock availability
            foreach (var cartItem in cart.CartItems)
            {
                if (cartItem.Product.StockQuantity < cartItem.Quantity)
                {
                    throw new Exception($"Insufficient stock for product: {cartItem.Product.Name}");
                }
            }

            var subTotal = cart.CartItems.Sum(ci => ci.Price * ci.Quantity);
            var shippingCost = CalculateShippingCost(subTotal);
            var discount = 0m;

            // Apply coupon if provided
            if (!string.IsNullOrEmpty(orderDto.CouponCode))
            {
                discount = await ApplyCouponAsync(orderDto.CouponCode, subTotal);
            }

            var totalAmount = subTotal + shippingCost - discount;

            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                SubTotal = subTotal,
                ShippingCost = shippingCost,
                Discount = discount,
                TotalAmount = totalAmount,
                CouponCode = orderDto.CouponCode,
                PaymentMethod = orderDto.PaymentMethod,
                Notes = orderDto.Notes,
                ShippingAddressId = orderDto.ShippingAddressId,
                BillingAddressId = orderDto.BillingAddressId ?? orderDto.ShippingAddressId,
                OrderItems = cart.CartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Price,
                    TotalPrice = ci.Price * ci.Quantity
                }).ToList()
            };

            await _context.Orders.AddAsync(order);

            // Update stock
            foreach (var cartItem in cart.CartItems)
            {
                cartItem.Product.StockQuantity -= cartItem.Quantity;
                _context.Products.Update(cartItem.Product);
            }

            // Clear cart
            _context.CartItems.RemoveRange(cart.CartItems);

            // Create invoice
            var invoice = new Invoice
            {
                InvoiceNumber = GenerateInvoiceNumber(),
                OrderId = order.Id,
                TotalAmount = totalAmount
            };
            await _context.Invoices.AddAsync(invoice);

            await _context.SaveChangesAsync();

            return await GetOrderByIdAsync(order.Id, userId);
        }

        public async Task<OrderDto> CancelOrderAsync(int orderId, int userId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            if (order.UserId != userId)
            {
                throw new UnauthorizedAccessException("You don't have permission to cancel this order");
            }

            if (order.Status != "Processing")
            {
                throw new Exception("Only orders with 'Processing' status can be cancelled");
            }

            order.Status = "Cancelled";
            order.PaymentStatus = "Refunded";

            // Restore stock
            foreach (var orderItem in order.OrderItems)
            {
                orderItem.Product.StockQuantity += orderItem.Quantity;
                _context.Products.Update(orderItem.Product);
            }

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return await GetOrderByIdAsync(orderId, userId);
        }

        public async Task<PaginatedResultDto<OrderDto>> GetAllOrdersAsync(string status, int page, int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.Invoice)
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            query = query.OrderByDescending(o => o.OrderDate);

            var totalCount = await query.CountAsync();

            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var orderDtos = orders.Select(MapOrderToDto).ToList();

            return new PaginatedResultDto<OrderDto>
            {
                Items = orderDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatusUpdateDto statusDto)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            order.Status = statusDto.Status;

            if (statusDto.Status == "Shipped")
            {
                order.ShippedAt = DateTime.UtcNow;
            }
            else if (statusDto.Status == "Delivered")
            {
                order.DeliveredAt = DateTime.UtcNow;
                order.PaymentStatus = "Paid";
            }

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return await GetOrderByIdAsync(orderId, order.UserId, true);
        }

        public async Task<InvoiceDto> GetInvoiceAsync(int orderId, int userId, bool isAdmin = false)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Order)
                .FirstOrDefaultAsync(i => i.OrderId == orderId);

            if (invoice == null)
            {
                throw new KeyNotFoundException("Invoice not found");
            }

            if (!isAdmin && invoice.Order.UserId != userId)
            {
                throw new UnauthorizedAccessException("You don't have permission to view this invoice");
            }

            return new InvoiceDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                IssueDate = invoice.IssueDate,
                TotalAmount = invoice.TotalAmount,
                PdfUrl = invoice.PdfUrl
            };
        }

        private OrderDto MapOrderToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                SubTotal = order.SubTotal,
                ShippingCost = order.ShippingCost,
                Discount = order.Discount,
                TotalAmount = order.TotalAmount,
                CouponCode = order.CouponCode,
                OrderDate = order.OrderDate,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                ShippedAt = order.ShippedAt,
                DeliveredAt = order.DeliveredAt,
                ShippingAddress = _mapper.Map<AddressDto>(order.ShippingAddress),
                BillingAddress = _mapper.Map<AddressDto>(order.BillingAddress),
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.Product.Id,
                    ProductName = oi.Product.Name,
                    ProductImageUrl = oi.Product.ImageUrl,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList(),
                Invoice = order.Invoice != null ? new InvoiceDto
                {
                    Id = order.Invoice.Id,
                    InvoiceNumber = order.Invoice.InvoiceNumber,
                    IssueDate = order.Invoice.IssueDate,
                    TotalAmount = order.Invoice.TotalAmount,
                    PdfUrl = order.Invoice.PdfUrl
                } : null
            };
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private decimal CalculateShippingCost(decimal subTotal)
        {
            if (subTotal >= 10000) return 0; // Free shipping over 10000
            if (subTotal >= 5000) return 500;
            return 1000;
        }

        private async Task<decimal> ApplyCouponAsync(string couponCode, decimal orderAmount)
        {
            // This would integrate with a CouponService
            // For now, return 0
            return 0;
        }
    }
}