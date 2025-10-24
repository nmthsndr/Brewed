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
        Task<OrderDto> CreateOrderAsync(int userId, OrderCreateDto orderCreateDto);
        Task<OrderDto> CreateGuestOrderAsync(GuestOrderCreateDto guestOrderCreateDto);
        Task<OrderDto> CancelOrderAsync(int orderId, int userId);
        Task<PaginatedResultDto<OrderDto>> GetAllOrdersAsync(string status, int page, int pageSize);
        Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatusUpdateDto statusDto);
        Task<InvoiceDto> GetInvoiceAsync(int orderId, int userId, bool isAdmin = false);
        Task<InvoiceDto> GenerateInvoiceAsync(int orderId);
        Task<bool> HasUserPurchasedProductAsync(int userId, int productId);
    }

    public class OrderService : IOrderService
    {
        private readonly BrewedDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICouponService _couponService;
        private readonly IEmailService _emailService;

        public OrderService(BrewedDbContext context, IMapper mapper, ICouponService couponService, IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _couponService = couponService;
            _emailService = emailService;
        }

        public async Task<List<OrderDto>> GetUserOrdersAsync(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.Invoice)
                .Include(o => o.User)
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
                .Include(o => o.User)
                .Include(o => o.GuestOrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            // Allow access if admin, or if user owns the order, or if it's a guest order (userId is null)
            if (!isAdmin && order.UserId.HasValue && order.UserId != userId)
            {
                throw new UnauthorizedAccessException("You don't have permission to view this order");
            }

            return MapOrderToDto(order);
        }

        public async Task<OrderDto> CreateOrderAsync(int userId, OrderCreateDto orderCreateDto)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                throw new Exception("Cart is empty");
            }

            var shippingAddress = await _context.Addresses.FindAsync(orderCreateDto.ShippingAddressId);
            if (shippingAddress == null || shippingAddress.UserId != userId)
            {
                throw new KeyNotFoundException("Shipping address not found");
            }

            Address billingAddress = null;
            if (orderCreateDto.BillingAddressId.HasValue)
            {
                billingAddress = await _context.Addresses.FindAsync(orderCreateDto.BillingAddressId.Value);
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
            if (!string.IsNullOrEmpty(orderCreateDto.CouponCode))
            {
                discount = await ApplyCouponAsync(orderCreateDto.CouponCode, subTotal);
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
                CouponCode = orderCreateDto.CouponCode,
                PaymentMethod = orderCreateDto.PaymentMethod,
                Notes = orderCreateDto.Notes,
                ShippingAddressId = orderCreateDto.ShippingAddressId,
                BillingAddressId = orderCreateDto.BillingAddressId ?? orderCreateDto.ShippingAddressId,
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

            // Save order
            await _context.SaveChangesAsync();

            // Send order confirmation email
            var orderDto = await GetOrderByIdAsync(order.Id, userId);
            await _emailService.SendOrderConfirmationAsync(orderDto);

            return orderDto;
        }

        public async Task<OrderDto> CreateGuestOrderAsync(GuestOrderCreateDto guestOrderCreateDto)
        {
            // Get guest cart
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.SessionId == guestOrderCreateDto.SessionId);

            if (cart == null || !cart.CartItems.Any())
            {
                throw new Exception("Cart is empty");
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
            if (!string.IsNullOrEmpty(guestOrderCreateDto.CouponCode))
            {
                discount = await ApplyCouponAsync(guestOrderCreateDto.CouponCode, subTotal);
            }

            var totalAmount = subTotal + shippingCost - discount;

            // Serialize guest addresses to JSON for storage
            var shippingAddressJson = System.Text.Json.JsonSerializer.Serialize(guestOrderCreateDto.ShippingAddress);
            var billingAddressJson = System.Text.Json.JsonSerializer.Serialize(guestOrderCreateDto.BillingAddress);

            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = null, // Guest order - no user ID
                SubTotal = subTotal,
                ShippingCost = shippingCost,
                Discount = discount,
                TotalAmount = totalAmount,
                CouponCode = guestOrderCreateDto.CouponCode,
                PaymentMethod = guestOrderCreateDto.PaymentMethod,
                Notes = guestOrderCreateDto.Notes,
                GuestEmail = guestOrderCreateDto.Email,
                GuestShippingAddress = shippingAddressJson,
                GuestBillingAddress = billingAddressJson,
                ShippingAddressId = null,
                BillingAddressId = null,
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

            // Save order first to get the order ID
            await _context.SaveChangesAsync();

            // Create and save guest order details
            var guestOrderDetails = new GuestOrderDetails
            {
                OrderId = order.Id,
                Email = guestOrderCreateDto.Email,
                FirstName = guestOrderCreateDto.ShippingAddress.FirstName,
                LastName = guestOrderCreateDto.ShippingAddress.LastName,
                ShippingAddressLine1 = guestOrderCreateDto.ShippingAddress.AddressLine1,
                ShippingAddressLine2 = guestOrderCreateDto.ShippingAddress.AddressLine2,
                ShippingCity = guestOrderCreateDto.ShippingAddress.City,
                ShippingPostalCode = guestOrderCreateDto.ShippingAddress.PostalCode,
                ShippingCountry = guestOrderCreateDto.ShippingAddress.Country,
                ShippingPhoneNumber = guestOrderCreateDto.ShippingAddress.PhoneNumber,
                BillingAddressLine1 = guestOrderCreateDto.BillingAddress.AddressLine1,
                BillingAddressLine2 = guestOrderCreateDto.BillingAddress.AddressLine2,
                BillingCity = guestOrderCreateDto.BillingAddress.City,
                BillingPostalCode = guestOrderCreateDto.BillingAddress.PostalCode,
                BillingCountry = guestOrderCreateDto.BillingAddress.Country,
                BillingPhoneNumber = guestOrderCreateDto.BillingAddress.PhoneNumber
            };

            await _context.GuestOrderDetails.AddAsync(guestOrderDetails);
            await _context.SaveChangesAsync();

            // Reload order with items for email
            var savedOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.GuestOrderDetails)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            // Send order confirmation email to guest
            var orderDto = MapGuestOrderToDto(savedOrder, guestOrderCreateDto);
            await _emailService.SendOrderConfirmationAsync(orderDto);

            return orderDto;
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

            // Check if user has permission to cancel (must be their order)
            if (!order.UserId.HasValue || order.UserId != userId)
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
                .Include(o => o.GuestOrderDetails)
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
            var order = await _context.Orders
                .Include(o => o.Invoice)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            // Check if invoice exists before allowing Shipped or Delivered status
            if ((statusDto.Status == "Shipped" || statusDto.Status == "Delivered") && order.Invoice == null)
            {
                throw new InvalidOperationException("Cannot ship or deliver order without generating an invoice first. Please generate invoice before updating status.");
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

            // Send email notification
            if (order.UserId.HasValue)
            {
                // Registered user order
                var user = await _context.Users.FindAsync(order.UserId.Value);
                if (user != null)
                {
                    await _emailService.SendOrderStatusUpdateAsync(user.Email, user.Name, order.OrderNumber, order.Status);
                }
            }
            else
            {
                // Guest order
                var guestDetails = await _context.GuestOrderDetails.FirstOrDefaultAsync(g => g.OrderId == orderId);
                if (guestDetails != null)
                {
                    await _emailService.SendOrderStatusUpdateAsync(
                        guestDetails.Email,
                        $"{guestDetails.FirstName} {guestDetails.LastName}",
                        order.OrderNumber,
                        order.Status
                    );
                }
            }

            return await GetOrderByIdAsync(orderId, order.UserId ?? 0, true);
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

            // Allow access if admin, or if user owns the order (guest orders can't access invoices)
            if (!isAdmin && invoice.Order.UserId.HasValue && invoice.Order.UserId != userId)
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
            // For guest orders, use GuestOrderDetails for address info
            AddressDto shippingAddress = null;
            AddressDto billingAddress = null;
            OrderUserDto user = null;

            if (order.UserId.HasValue && order.User != null)
            {
                // Regular user order
                shippingAddress = _mapper.Map<AddressDto>(order.ShippingAddress);
                billingAddress = _mapper.Map<AddressDto>(order.BillingAddress);
                user = new OrderUserDto
                {
                    Id = order.User.Id,
                    Name = order.User.Name,
                    Email = order.User.Email
                };
            }
            else if (order.GuestOrderDetails != null)
            {
                // Guest order - map from GuestOrderDetails
                var guestDetails = order.GuestOrderDetails;
                shippingAddress = new AddressDto
                {
                    FirstName = guestDetails.FirstName,
                    LastName = guestDetails.LastName,
                    AddressLine1 = guestDetails.ShippingAddressLine1,
                    AddressLine2 = guestDetails.ShippingAddressLine2,
                    City = guestDetails.ShippingCity,
                    PostalCode = guestDetails.ShippingPostalCode,
                    Country = guestDetails.ShippingCountry,
                    PhoneNumber = guestDetails.ShippingPhoneNumber,
                    AddressType = "Shipping"
                };
                billingAddress = new AddressDto
                {
                    FirstName = guestDetails.FirstName,
                    LastName = guestDetails.LastName,
                    AddressLine1 = guestDetails.BillingAddressLine1,
                    AddressLine2 = guestDetails.BillingAddressLine2,
                    City = guestDetails.BillingCity,
                    PostalCode = guestDetails.BillingPostalCode,
                    Country = guestDetails.BillingCountry,
                    PhoneNumber = guestDetails.BillingPhoneNumber,
                    AddressType = "Billing"
                };
                user = new OrderUserDto
                {
                    Id = 0,
                    Name = $"{guestDetails.FirstName} {guestDetails.LastName}",
                    Email = guestDetails.Email
                };
            }

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
                ShippingAddress = shippingAddress,
                BillingAddress = billingAddress,
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
                } : null,
                User = user
            };
        }

        private OrderDto MapGuestOrderToDto(Order order, GuestOrderCreateDto guestOrderDto)
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
                ShippingAddress = new AddressDto
                {
                    FirstName = guestOrderDto.ShippingAddress.FirstName,
                    LastName = guestOrderDto.ShippingAddress.LastName,
                    AddressLine1 = guestOrderDto.ShippingAddress.AddressLine1,
                    AddressLine2 = guestOrderDto.ShippingAddress.AddressLine2,
                    City = guestOrderDto.ShippingAddress.City,
                    PostalCode = guestOrderDto.ShippingAddress.PostalCode,
                    Country = guestOrderDto.ShippingAddress.Country,
                    PhoneNumber = guestOrderDto.ShippingAddress.PhoneNumber,
                    AddressType = "Shipping"
                },
                BillingAddress = new AddressDto
                {
                    FirstName = guestOrderDto.BillingAddress.FirstName,
                    LastName = guestOrderDto.BillingAddress.LastName,
                    AddressLine1 = guestOrderDto.BillingAddress.AddressLine1,
                    AddressLine2 = guestOrderDto.BillingAddress.AddressLine2,
                    City = guestOrderDto.BillingAddress.City,
                    PostalCode = guestOrderDto.BillingAddress.PostalCode,
                    Country = guestOrderDto.BillingAddress.Country,
                    PhoneNumber = guestOrderDto.BillingAddress.PhoneNumber,
                    AddressType = "Billing"
                },
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
                Invoice = null,
                User = new OrderUserDto
                {
                    Id = 0,
                    Name = $"{guestOrderDto.ShippingAddress.FirstName} {guestOrderDto.ShippingAddress.LastName}",
                    Email = guestOrderDto.Email
                }
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
            if (subTotal >= 50) return 0; // Free shipping over 50 euros
            return 10;
        }

        private async Task<decimal> ApplyCouponAsync(string couponCode, decimal orderAmount)
        {
            return await _couponService.ApplyCouponAsync(couponCode, orderAmount);
        }

        public async Task<InvoiceDto> GenerateInvoiceAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Invoice)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            // Check if invoice already exists
            if (order.Invoice != null)
            {
                throw new InvalidOperationException("Invoice already exists for this order");
            }

            // Create new invoice
            var invoice = new Invoice
            {
                InvoiceNumber = GenerateInvoiceNumber(),
                OrderId = order.Id,
                TotalAmount = order.TotalAmount,
                PdfUrl = string.Empty // Will be generated later
            };

            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();

            // Send invoice email
            var orderDto = await GetOrderByIdAsync(orderId, order.UserId, true);
            await _emailService.SendInvoiceEmailAsync(orderDto);

            return new InvoiceDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                IssueDate = invoice.IssueDate,
                TotalAmount = invoice.TotalAmount,
                PdfUrl = invoice.PdfUrl
            };
        }

        public async Task<bool> HasUserPurchasedProductAsync(int userId, int productId)
        {
            // Check if user has any delivered orders containing this product
            var hasPurchased = await _context.Orders
                .Include(o => o.OrderItems)
                .AnyAsync(o => o.UserId == userId
                    && o.Status == "Delivered"
                    && o.OrderItems.Any(oi => oi.ProductId == productId));

            return hasPurchased;
        }
    }
}