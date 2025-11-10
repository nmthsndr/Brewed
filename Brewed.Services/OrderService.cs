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

            // Allow access if admin, or if user owns the order
            if (!isAdmin && order.UserId != userId)
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
                discount = await ApplyCouponAsync(orderCreateDto.CouponCode, subTotal, userId);
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

                // Send low stock alert if stock is below threshold (e.g., 10)
                if (cartItem.Product.StockQuantity <= 10 && cartItem.Product.StockQuantity > 0)
                {
                    try
                    {
                        // Get all admin users from database
                        var adminEmails = await _context.Users
                            .Where(u => u.Role == "Admin")
                            .Select(u => u.Email)
                            .ToListAsync();

                        await _emailService.SendLowStockAlertAsync(cartItem.Product.Name, cartItem.Product.StockQuantity, adminEmails);
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't fail the order
                        Console.WriteLine($"Failed to send low stock alert for {cartItem.Product.Name}: {ex.Message}");
                    }
                }
            }

            // Clear cart
            _context.CartItems.RemoveRange(cart.CartItems);

            // Save order
            await _context.SaveChangesAsync();

            // Mark coupon as used if provided
            if (!string.IsNullOrEmpty(orderCreateDto.CouponCode))
            {
                try
                {
                    await _couponService.MarkCouponAsUsedAsync(userId, orderCreateDto.CouponCode, order.Id);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the order
                    Console.WriteLine($"Failed to mark coupon as used: {ex.Message}");
                }
            }

            // Send order confirmation email
            var orderDto = await GetOrderByIdAsync(order.Id, userId);
            await _emailService.SendOrderConfirmationAsync(orderDto);

            return orderDto;
        }

        public async Task<OrderDto> CreateGuestOrderAsync(GuestOrderCreateDto guestOrderCreateDto)
        {
            // Check if email already exists in the database
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == guestOrderCreateDto.Email);

            if (existingUser != null)
            {
                if (existingUser.Role == "Guest")
                {
                    throw new Exception("This email is already associated with a guest order. Please use a different email or log in if you have an account.");
                }
                else
                {
                    throw new Exception("This email is already registered. Please log in to place your order.");
                }
            }

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

            // Create a temporary guest user (no password, not verified)
            var guestUser = new User
            {
                Name = $"{guestOrderCreateDto.ShippingAddress.FirstName} {guestOrderCreateDto.ShippingAddress.LastName}",
                Email = guestOrderCreateDto.Email,
                PasswordHash = string.Empty, // No password for guest users
                Role = "Guest",
                EmailConfirmed = false
            };
            await _context.Users.AddAsync(guestUser);
            await _context.SaveChangesAsync(); // Save to get the UserId

            // Create shipping address
            var shippingAddress = new Address
            {
                FirstName = guestOrderCreateDto.ShippingAddress.FirstName,
                LastName = guestOrderCreateDto.ShippingAddress.LastName,
                AddressLine1 = guestOrderCreateDto.ShippingAddress.AddressLine1,
                AddressLine2 = guestOrderCreateDto.ShippingAddress.AddressLine2,
                City = guestOrderCreateDto.ShippingAddress.City,
                PostalCode = guestOrderCreateDto.ShippingAddress.PostalCode,
                Country = guestOrderCreateDto.ShippingAddress.Country,
                PhoneNumber = guestOrderCreateDto.ShippingAddress.PhoneNumber,
                UserId = null // Guest address - not linked to user profile
            };
            await _context.Addresses.AddAsync(shippingAddress);

            // Create billing address
            var billingAddress = new Address
            {
                FirstName = guestOrderCreateDto.BillingAddress.FirstName,
                LastName = guestOrderCreateDto.BillingAddress.LastName,
                AddressLine1 = guestOrderCreateDto.BillingAddress.AddressLine1,
                AddressLine2 = guestOrderCreateDto.BillingAddress.AddressLine2,
                City = guestOrderCreateDto.BillingAddress.City,
                PostalCode = guestOrderCreateDto.BillingAddress.PostalCode,
                Country = guestOrderCreateDto.BillingAddress.Country,
                PhoneNumber = guestOrderCreateDto.BillingAddress.PhoneNumber,
                UserId = null // Guest address - not linked to user profile
            };
            await _context.Addresses.AddAsync(billingAddress);
            await _context.SaveChangesAsync(); // Save to get address IDs

            // Create the order with all required IDs
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = guestUser.Id,
                SubTotal = subTotal,
                ShippingCost = shippingCost,
                Discount = discount,
                TotalAmount = totalAmount,
                CouponCode = guestOrderCreateDto.CouponCode,
                PaymentMethod = guestOrderCreateDto.PaymentMethod,
                Notes = guestOrderCreateDto.Notes,
                ShippingAddressId = shippingAddress.Id,
                BillingAddressId = billingAddress.Id,
                IsGuestOrder = true, // Mark as guest order
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

                // Send low stock alert if stock is below threshold (e.g., 10)
                if (cartItem.Product.StockQuantity <= 10 && cartItem.Product.StockQuantity > 0)
                {
                    try
                    {
                        // Get all admin users from database
                        var adminEmails = await _context.Users
                            .Where(u => u.Role == "Admin")
                            .Select(u => u.Email)
                            .ToListAsync();

                        await _emailService.SendLowStockAlertAsync(cartItem.Product.Name, cartItem.Product.StockQuantity, adminEmails);
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't fail the order
                        Console.WriteLine($"Failed to send low stock alert for {cartItem.Product.Name}: {ex.Message}");
                    }
                }
            }

            // Clear cart
            _context.CartItems.RemoveRange(cart.CartItems);

            // Save order first to get the order ID
            await _context.SaveChangesAsync();

            // Create and save guest order details for easy querying
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

            // Reload order with all relationships for email
            var savedOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.GuestOrderDetails)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            // Send order confirmation email to guest
            var orderDto = MapOrderToDto(savedOrder);
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
            var user = await _context.Users.FindAsync(order.UserId);
            if (user != null)
            {
                await _emailService.SendOrderStatusUpdateAsync(user.Email, user.Name, order.OrderNumber, order.Status);
            }

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

            // Allow access if admin, or if user owns the order
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
                } : null,
                User = order.User != null ? new OrderUserDto
                {
                    Id = order.User.Id,
                    Name = order.User.Name,
                    Email = order.User.Email
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
            if (subTotal >= 50) return 0; // Free shipping over 50 euros
            return 10;
        }

        private async Task<decimal> ApplyCouponAsync(string couponCode, decimal orderAmount, int? userId = null)
        {
            // If userId is provided, validate that the user has access to this coupon
            if (userId.HasValue)
            {
                var validation = await _couponService.ValidateCouponForUserAsync(userId.Value, couponCode, orderAmount);
                if (!validation.IsValid)
                {
                    throw new Exception(validation.Message);
                }
                return validation.DiscountAmount;
            }
            else
            {
                // Guest orders - use regular validation (no user restriction)
                return await _couponService.ApplyCouponAsync(couponCode, orderAmount);
            }
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