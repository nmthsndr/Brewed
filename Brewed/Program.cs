namespace Brewed
{
    using Brewed.DataContext.Context;
    using Brewed.DataContext.Entities;
    using Brewed.Services;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;
    using System.Text;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Database context
            builder.Services.AddDbContext<BrewedDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("Brewed.DataContext")
                ));

            // AutoMapper
            builder.Services.AddAutoMapper(cfg => {
                cfg.AddProfile<AutoMapperProfile>();
            });

            // Services
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAddressService, AddressService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<ICouponService, CouponService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IPdfService, PdfService>();

            builder.Services.AddScoped<IFileUploadService>(provider =>
            {
                var env = provider.GetRequiredService<IWebHostEnvironment>();
                // Pass WebRootPath or null - FileUploadService will handle null case
                return new FileUploadService(env.WebRootPath ?? string.Empty);
            });

            // JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Brewed API",
                    Version = "v1",
                    Description = "Coffee Shop API - Complete Documentation"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header. Enter: {token}"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Brewed API v1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseCors("AllowAll");
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // Database seeding
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<BrewedDbContext>();
                    context.Database.Migrate();
                    SeedData(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred during database initialization");
                }
            }

            app.Run();
        }

        public static void SeedData(BrewedDbContext context)
        {
            if (context.Categories.Any())
                return;

            using var tx = context.Database.BeginTransaction();

            // Categories - Coffee beans and accessories
            var categories = new List<Category>
    {
        new() { Name = "Coffee Beans", Description = "Premium coffee beans from around the world" },
        new() { Name = "Espresso Machines", Description = "Professional and home espresso machines" },
        new() { Name = "Coffee Grinders", Description = "Manual and electric coffee grinders" },
        new() { Name = "Brewing Equipment", Description = "Pour over, French press, and other brewing tools" },
        new() { Name = "Scales & Timers", Description = "Precision scales and timers for perfect brewing" },
        new() { Name = "Milk Frothers", Description = "Manual and electric milk frothers and steamers" },
        new() { Name = "Accessories", Description = "Cups, filters, cleaning supplies, and more" }
    };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            var products = new List<Product>
    {
        // Coffee Beans (Category 1)
        new() { Name = "Ethiopian Yirgacheffe", Description = "Floral and citrus notes with bright acidity. Perfect for pour-over brewing. 250g bag.", Price = 14.90m, StockQuantity = 45, ImageUrl = "/images/beans-ethiopian.jpg", RoastLevel="Light", Origin="Ethiopia", IsOrganic=true, CategoryId = 1 },
        new() { Name = "Colombian Supremo", Description = "Balanced body with caramel sweetness and nutty undertones. 250g bag.", Price = 12.90m, StockQuantity = 60, ImageUrl = "/images/beans-colombian.jpg", RoastLevel="Medium", Origin="Colombia", CategoryId = 1 },
        new() { Name = "Brazilian Santos", Description = "Smooth, low-acidity with chocolate notes. Great for espresso. 250g bag.", Price = 11.90m, StockQuantity = 70, ImageUrl = "/images/beans-brazilian.jpg", RoastLevel="Medium-Dark", Origin="Brazil", CategoryId = 1 },
        new() { Name = "Guatemalan Antigua", Description = "Rich, full-bodied with spicy and smoky notes. 250g bag.", Price = 13.90m, StockQuantity = 8, ImageUrl = "/images/beans-guatemalan.jpg", RoastLevel="Dark", Origin="Guatemala", IsOrganic=true, CategoryId = 1 },
        new() { Name = "Kenyan AA", Description = "Bold, wine-like acidity with berry and black currant flavors. 250g bag.", Price = 16.90m, StockQuantity = 40, ImageUrl = "/images/beans-kenyan.jpg", RoastLevel="Medium", Origin="Kenya", CategoryId = 1 },
        new() { Name = "Costa Rican Tarrazu", Description = "Clean, crisp with bright citrus acidity. 250g bag.", Price = 13.50m, StockQuantity = 55, ImageUrl = "/images/beans-costarican.jpg", RoastLevel="Light-Medium", Origin="Costa Rica", IsOrganic=true, CategoryId = 1 },
        new() { Name = "Sumatra Mandheling", Description = "Earthy, herbal with low acidity and full body. 250g bag.", Price = 12.50m, StockQuantity = 50, ImageUrl = "/images/beans-sumatra.jpg", RoastLevel="Dark", Origin="Indonesia", CategoryId = 1 },
        new() { Name = "Decaf Swiss Water", Description = "Smooth decaf with no chemical residue. Medium roast blend. 250g bag.", Price = 11.90m, StockQuantity = 35, ImageUrl = "/images/beans-decaf.jpg", RoastLevel="Medium", Origin="Colombia", IsCaffeineFree=true, CategoryId = 1 },
        new() { Name = "Espresso Blend", Description = "Rich, balanced blend perfect for espresso machines. 1kg bag.", Price = 32.90m, StockQuantity = 80, ImageUrl = "/images/beans-espresso.jpg", RoastLevel="Dark", Origin="Blend", CategoryId = 1 },
        new() { Name = "House Blend", Description = "Our signature blend - smooth and versatile. 1kg bag.", Price = 28.90m, StockQuantity = 100, ImageUrl = "/images/beans-house.jpg", RoastLevel="Medium", Origin="Blend", CategoryId = 1 },

        // Espresso Machines (Category 2)
        new() { Name = "Breville Barista Express", Description = "All-in-one espresso machine with built-in grinder. Perfect for home baristas.", Price = 599.00m, StockQuantity = 15, ImageUrl = "/images/breville-barista.jpg", RoastLevel="N/A", Origin="Australia", CategoryId = 2 },
        new() { Name = "Gaggia Classic Pro", Description = "Traditional Italian espresso machine with commercial-style portafilter.", Price = 449.00m, StockQuantity = 12, ImageUrl = "/images/gaggia-classic.jpg", RoastLevel="N/A", Origin="Italy", CategoryId = 2 },
        new() { Name = "De'Longhi Dedica", Description = "Slim and compact espresso machine, perfect for small kitchens.", Price = 299.00m, StockQuantity = 20, ImageUrl = "/images/delonghi-dedica.jpg", RoastLevel="N/A", Origin="Italy", CategoryId = 2 },
        new() { Name = "Rancilio Silvia", Description = "Professional-grade home espresso machine with brass boiler.", Price = 749.00m, StockQuantity = 8, ImageUrl = "/images/rancilio-silvia.jpg", RoastLevel="N/A", Origin="Italy", CategoryId = 2 },
        new() { Name = "Nespresso Vertuo", Description = "Pod-based espresso and coffee maker with automatic capsule recognition.", Price = 179.00m, StockQuantity = 25, ImageUrl = "/images/nespresso-vertuo.jpg", RoastLevel="N/A", Origin="Switzerland", CategoryId = 2 },

        // Coffee Grinders (Category 3)
        new() { Name = "Baratza Encore", Description = "Entry-level burr grinder with 40 grind settings. Perfect for home use.", Price = 139.00m, StockQuantity = 30, ImageUrl = "/images/baratza-encore.jpg", RoastLevel="N/A", Origin="USA", CategoryId = 3 },
        new() { Name = "Comandante C40 MK4", Description = "Premium manual hand grinder with stainless steel conical burrs.", Price = 249.00m, StockQuantity = 18, ImageUrl = "/images/comandante.jpg", RoastLevel="N/A", Origin="Germany", CategoryId = 3 },
        new() { Name = "Timemore Chestnut C2", Description = "Portable manual grinder with adjustable grind settings.", Price = 69.00m, StockQuantity = 35, ImageUrl = "/images/timemore-c2.jpg", RoastLevel="N/A", Origin="China", CategoryId = 3 },
        new() { Name = "Wilfa Svart", Description = "Precision electric grinder with flat ceramic burrs and 17 grind settings.", Price = 119.00m, StockQuantity = 22, ImageUrl = "/images/wilfa-svart.jpg", RoastLevel="N/A", Origin="Norway", CategoryId = 3 },
        new() { Name = "Eureka Mignon", Description = "Professional espresso grinder with 55mm flat burrs and stepless adjustment.", Price = 399.00m, StockQuantity = 5, ImageUrl = "/images/eureka-mignon.jpg", RoastLevel="N/A", Origin="Italy", CategoryId = 3 },

        // Brewing Equipment (Category 4)
        new() { Name = "Hario V60 Dripper", Description = "Ceramic pour-over coffee dripper with spiral ribs for optimal extraction.", Price = 24.90m, StockQuantity = 50, ImageUrl = "/images/hario-v60.jpg", RoastLevel="N/A", Origin="Japan", CategoryId = 4 },
        new() { Name = "Chemex Classic 6-Cup", Description = "Iconic glass pour-over coffee maker with bonded filters.", Price = 44.90m, StockQuantity = 28, ImageUrl = "/images/chemex.jpg", RoastLevel="N/A", Origin="USA", CategoryId = 4 },
        new() { Name = "Bodum Chambord French Press", Description = "Classic French press with stainless steel frame and borosilicate glass.", Price = 34.90m, StockQuantity = 40, ImageUrl = "/images/bodum-french-press.jpg", RoastLevel="N/A", Origin="Switzerland", CategoryId = 4 },
        new() { Name = "AeroPress Coffee Maker", Description = "Versatile coffee maker using air pressure for quick, clean brewing.", Price = 39.90m, StockQuantity = 45, ImageUrl = "/images/aeropress.jpg", RoastLevel="N/A", Origin="USA", CategoryId = 4 },
        new() { Name = "Bialetti Moka Express", Description = "Classic Italian stovetop espresso maker in aluminium. 6-cup size.", Price = 32.90m, StockQuantity = 35, ImageUrl = "/images/bialetti-moka.jpg", RoastLevel="N/A", Origin="Italy", CategoryId = 4 },
        new() { Name = "Kalita Wave 185", Description = "Flat-bottom dripper with wave filters for consistent extraction.", Price = 29.90m, StockQuantity = 32, ImageUrl = "/images/kalita-wave.jpg", RoastLevel="N/A", Origin="Japan", CategoryId = 4 },

        // Scales & Timers (Category 5)
        new() { Name = "Hario Drip Scale", Description = "Coffee scale with built-in timer, 0.1g accuracy, and auto-off function.", Price = 49.90m, StockQuantity = 38, ImageUrl = "/images/hario-scale.jpg", RoastLevel="N/A", Origin="Japan", CategoryId = 5 },
        new() { Name = "Acaia Pearl", Description = "Professional coffee scale with Bluetooth connectivity and flow-rate indicator.", Price = 219.00m, StockQuantity = 12, ImageUrl = "/images/acaia-pearl.jpg", RoastLevel="N/A", Origin="USA", CategoryId = 5 },
        new() { Name = "Timemore Black Mirror", Description = "Precision scale with LED display, auto-tare, and timer. 0.1g accuracy.", Price = 89.00m, StockQuantity = 25, ImageUrl = "/images/timemore-scale.jpg", RoastLevel="N/A", Origin="China", CategoryId = 5 },
        new() { Name = "Brewista Smart Scale II", Description = "Dual-display scale with auto-start timer and 0.1g precision.", Price = 69.90m, StockQuantity = 20, ImageUrl = "/images/brewista-scale.jpg", RoastLevel="N/A", Origin="USA", CategoryId = 5 },

        // Milk Frothers (Category 6)
        new() { Name = "Nanofoamer V2", Description = "Handheld milk frother creating microfoam in 10-15 seconds.", Price = 19.90m, StockQuantity = 60, ImageUrl = "/images/nanofoamer.jpg", RoastLevel="N/A", Origin="USA", CategoryId = 6 },
        new() { Name = "Bellman Steamer", Description = "Stovetop milk steamer producing professional-quality microfoam.", Price = 89.90m, StockQuantity = 15, ImageUrl = "/images/bellman-steamer.jpg", RoastLevel="N/A", Origin="Sweden", CategoryId = 6 },
        new() { Name = "Breville Milk Café", Description = "Automatic milk frother with hot and cold settings for various drinks.", Price = 129.00m, StockQuantity = 18, ImageUrl = "/images/breville-frother.jpg", RoastLevel="N/A", Origin="Australia", CategoryId = 6 },
        new() { Name = "Subminimal NanoFoamer", Description = "Premium handheld frother with replaceable tips and USB charging.", Price = 39.90m, StockQuantity = 42, ImageUrl = "/images/subminimal.jpg", RoastLevel="N/A", Origin="New Zealand", CategoryId = 6 },

        // Accessories (Category 7)
        new() { Name = "Barista Tamper 58mm", Description = "Stainless steel espresso tamper with ergonomic wooden handle.", Price = 29.90m, StockQuantity = 45, ImageUrl = "/images/tamper.jpg", RoastLevel="N/A", Origin="Italy", CategoryId = 7 },
        new() { Name = "Milk Jug 600ml", Description = "Stainless steel milk frothing jug with measurement markings.", Price = 19.90m, StockQuantity = 55, ImageUrl = "/images/milk-jug.jpg", RoastLevel="N/A", Origin="Italy", CategoryId = 7 },
        new() { Name = "Knock Box", Description = "Durable espresso knock box with non-slip rubber bar.", Price = 24.90m, StockQuantity = 30, ImageUrl = "/images/knock-box.jpg", RoastLevel="N/A", Origin="Italy", CategoryId = 7 },
        new() { Name = "Coffee Filters V60 (100pcs)", Description = "Premium paper filters for Hario V60 dripper, size 02.", Price = 6.90m, StockQuantity = 120, ImageUrl = "/images/filters-v60.jpg", RoastLevel="N/A", Origin="Japan", CategoryId = 7 },
        new() { Name = "Chemex Filters (100pcs)", Description = "Bonded paper filters for Chemex coffee makers.", Price = 9.90m, StockQuantity = 100, ImageUrl = "/images/filters-chemex.jpg", RoastLevel="N/A", Origin="USA", CategoryId = 7 },
        new() { Name = "Cleaning Tablets (100pcs)", Description = "Professional espresso machine cleaning tablets for backflushing.", Price = 14.90m, StockQuantity = 80, ImageUrl = "/images/cleaning-tablets.jpg", RoastLevel="N/A", Origin="USA", CategoryId = 7 },
        new() { Name = "Barista Cloth Set", Description = "Set of 3 microfiber cloths for cleaning espresso machines and equipment.", Price = 12.90m, StockQuantity = 65, ImageUrl = "/images/barista-cloth.jpg", RoastLevel="N/A", Origin="Germany", CategoryId = 7 },
        new() { Name = "Double Wall Espresso Cups", Description = "Set of 2 thermal double-wall glass espresso cups (80ml).", Price = 19.90m, StockQuantity = 40, ImageUrl = "/images/espresso-cups.jpg", RoastLevel="N/A", Origin="Italy", CategoryId = 7 },
        new() { Name = "Latte Art Pen", Description = "Stainless steel latte art pen for creating detailed designs.", Price = 8.90m, StockQuantity = 50, ImageUrl = "/images/latte-pen.jpg", RoastLevel="N/A", Origin="Italy", CategoryId = 7 },
        new() { Name = "Coffee Canister 500g", Description = "Airtight stainless steel container with CO2 valve for coffee storage.", Price = 24.90m, StockQuantity = 38, ImageUrl = "/images/coffee-canister.jpg", RoastLevel="N/A", Origin="USA", CategoryId = 7 }
    };
            context.Products.AddRange(products);
            context.SaveChanges();

            // Create Admin User
            var adminUser = new User
            {
                Name = "Admin User",
                Email = "admin@brewed.com",
                PasswordHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("Admin123!"))),
                Role = "Admin",
                EmailConfirmed = true
            };
            context.Users.Add(adminUser);
            context.SaveChanges();

            // Create Regular Users
            var users = new List<User>
    {
        new() { Name="Sarah Miller", Email="sarah.miller@example.com", PasswordHash=Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("Password123"))), Role="RegisteredUser", EmailConfirmed=true },
        new() { Name="James Brown", Email="james.brown@example.com", PasswordHash=Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("Password123"))), Role="RegisteredUser", EmailConfirmed=true },
        new() { Name="Emma Wilson", Email="emma.wilson@example.com", PasswordHash=Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("Password123"))), Role="RegisteredUser", EmailConfirmed=true },
        new() { Name="Michael Davis", Email="michael.davis@example.com", PasswordHash=Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("Password123"))), Role="RegisteredUser", EmailConfirmed=true },
        new() { Name="Lisa Anderson", Email="lisa.anderson@example.com", PasswordHash=Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("Password123"))), Role="RegisteredUser", EmailConfirmed=true }
    };
            context.Users.AddRange(users);
            context.SaveChanges();

            // Create Addresses for Users
            var addresses = new List<Address>
    {
        new() { FirstName = "Sarah", LastName = "Miller", AddressLine1 = "Kurfürstendamm 123", City = "Berlin", PostalCode = "10719", Country = "Germany", PhoneNumber = "+49 30 12345678", IsDefault = true, UserId = users[0].Id },
        new() { FirstName = "James", LastName = "Brown", AddressLine1 = "Leopoldstraße 45", City = "Munich", PostalCode = "80802", Country = "Germany", PhoneNumber = "+49 89 98765432", IsDefault = true, UserId = users[1].Id },
        new() { FirstName = "Emma", LastName = "Wilson", AddressLine1 = "Zeil 78", City = "Frankfurt", PostalCode = "60313", Country = "Germany", PhoneNumber = "+49 69 55544433", IsDefault = true, UserId = users[2].Id },
        new() { FirstName = "Michael", LastName = "Davis", AddressLine1 = "Mönckebergstraße 12", City = "Hamburg", PostalCode = "20095", Country = "Germany", PhoneNumber = "+49 40 22334455", IsDefault = true, UserId = users[3].Id },
        new() { FirstName = "Lisa", LastName = "Anderson", AddressLine1 = "Königsallee 89", City = "Düsseldorf", PostalCode = "40212", Country = "Germany", PhoneNumber = "+49 211 66778899", IsDefault = true, UserId = users[4].Id },
    };
            context.Addresses.AddRange(addresses);
            context.SaveChanges();

            // Create Coupons
            var coupons = new List<Coupon>
    {
        new() { Code = "WELCOME10", Description = "10% discount for new customers", DiscountType = "Percentage", DiscountValue = 10m, MinimumOrderAmount = 20m, StartDate = DateTime.UtcNow.AddDays(-30), EndDate = DateTime.UtcNow.AddDays(30), IsActive = true },
        new() { Code = "BEANS20", Description = "20% off all coffee beans", DiscountType = "Percentage", DiscountValue = 20m, MinimumOrderAmount = 30m, StartDate = DateTime.UtcNow.AddDays(-10), EndDate = DateTime.UtcNow.AddDays(60), IsActive = true },
        new() { Code = "FREESHIP", Description = "Free shipping over 25€", DiscountType = "FixedAmount", DiscountValue = 10m, MinimumOrderAmount = 25m, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(90), IsActive = true },
        new() { Code = "GEAR15", Description = "15% off coffee equipment", DiscountType = "Percentage", DiscountValue = 15m, MinimumOrderAmount = 100m, StartDate = DateTime.UtcNow.AddDays(-5), EndDate = DateTime.UtcNow.AddDays(45), IsActive = true }
    };
            context.Coupons.AddRange(coupons);
            context.SaveChanges();

            // Create Sample Orders with different statuses
            var orders = new List<Order>
    {
        // Order 1: Delivered (Sarah - 3 months ago)
        new() {
            OrderNumber = $"ORD-{DateTime.UtcNow.AddMonths(-3):yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            UserId = users[0].Id,
            OrderDate = DateTime.UtcNow.AddMonths(-3),
            SubTotal = 44.80m,
            ShippingCost = 5m,
            Discount = 0m,
            TotalAmount = 49.80m,
            CouponCode = "",
            Status = "Delivered",
            PaymentMethod = "CreditCard",
            PaymentStatus = "Paid",
            Notes = "",
            ShippingAddressId = addresses[0].Id,
            BillingAddressId = addresses[0].Id,
            ShippedAt = DateTime.UtcNow.AddMonths(-3).AddDays(2),
            DeliveredAt = DateTime.UtcNow.AddMonths(-3).AddDays(4),
            OrderItems = new List<OrderItem> {
                new() { ProductId = 1, Quantity = 2, UnitPrice = 14.90m, TotalPrice = 29.80m },
                new() { ProductId = 3, Quantity = 1, UnitPrice = 11.90m, TotalPrice = 11.90m },
                new() { ProductId = 41, Quantity = 1, UnitPrice = 6.90m, TotalPrice = 6.90m }
            }
        },
        // Order 2: Delivered (James - 2 months ago)
        new() {
            OrderNumber = $"ORD-{DateTime.UtcNow.AddMonths(-2):yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            UserId = users[1].Id,
            OrderDate = DateTime.UtcNow.AddMonths(-2).AddDays(-5),
            SubTotal = 638.00m,
            ShippingCost = 5m,
            Discount = 0m,
            TotalAmount = 643.00m,
            CouponCode = "",
            Status = "Delivered",
            PaymentMethod = "BankTransfer",
            PaymentStatus = "Paid",
            Notes = "",
            ShippingAddressId = addresses[1].Id,
            BillingAddressId = addresses[1].Id,
            ShippedAt = DateTime.UtcNow.AddMonths(-2).AddDays(-3),
            DeliveredAt = DateTime.UtcNow.AddMonths(-2).AddDays(-1),
            OrderItems = new List<OrderItem> {
                new() { ProductId = 11, Quantity = 1, UnitPrice = 599.00m, TotalPrice = 599.00m },
                new() { ProductId = 16, Quantity = 1, UnitPrice = 139.00m, TotalPrice = 139.00m }
            }
        },
        // Order 3: Delivered (Emma - 1.5 months ago)
        new() {
            OrderNumber = $"ORD-{DateTime.UtcNow.AddDays(-45):yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            UserId = users[2].Id,
            OrderDate = DateTime.UtcNow.AddDays(-45),
            SubTotal = 153.70m,
            ShippingCost = 5m,
            Discount = 0m,
            TotalAmount = 158.70m,
            CouponCode = "",
            Status = "Delivered",
            PaymentMethod = "CreditCard",
            PaymentStatus = "Paid",
            Notes = "",
            ShippingAddressId = addresses[2].Id,
            BillingAddressId = addresses[2].Id,
            ShippedAt = DateTime.UtcNow.AddDays(-43),
            DeliveredAt = DateTime.UtcNow.AddDays(-41),
            OrderItems = new List<OrderItem> {
                new() { ProductId = 21, Quantity = 1, UnitPrice = 44.90m, TotalPrice = 44.90m },
                new() { ProductId = 26, Quantity = 1, UnitPrice = 49.90m, TotalPrice = 49.90m },
                new() { ProductId = 2, Quantity = 2, UnitPrice = 12.90m, TotalPrice = 25.80m },
                new() { ProductId = 38, Quantity = 2, UnitPrice = 19.90m, TotalPrice = 39.80m }
            }
        },
        // Order 4: Shipped (Michael - 5 days ago)
        new() {
            OrderNumber = $"ORD-{DateTime.UtcNow.AddDays(-5):yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            UserId = users[3].Id,
            OrderDate = DateTime.UtcNow.AddDays(-5),
            SubTotal = 297.80m,
            ShippingCost = 5m,
            Discount = 0m,
            TotalAmount = 302.80m,
            CouponCode = "",
            Status = "Shipped",
            PaymentMethod = "DebitCard",
            PaymentStatus = "Paid",
            Notes = "",
            ShippingAddressId = addresses[3].Id,
            BillingAddressId = addresses[3].Id,
            ShippedAt = DateTime.UtcNow.AddDays(-2),
            OrderItems = new List<OrderItem> {
                new() { ProductId = 17, Quantity = 1, UnitPrice = 249.00m, TotalPrice = 249.00m },
                new() { ProductId = 5, Quantity = 2, UnitPrice = 16.90m, TotalPrice = 33.80m },
                new() { ProductId = 33, Quantity = 1, UnitPrice = 29.90m, TotalPrice = 29.90m }
            }
        },
        // Order 5: Processing (Lisa - 2 days ago)
        new() {
            OrderNumber = $"ORD-{DateTime.UtcNow.AddDays(-2):yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            UserId = users[4].Id,
            OrderDate = DateTime.UtcNow.AddDays(-2),
            SubTotal = 887.70m,
            ShippingCost = 5m,
            Discount = 88.77m,
            TotalAmount = 803.93m,
            CouponCode = "WELCOME10",
            Status = "Processing",
            PaymentMethod = "CreditCard",
            PaymentStatus = "Pending",
            Notes = "",
            ShippingAddressId = addresses[4].Id,
            BillingAddressId = addresses[4].Id,
            OrderItems = new List<OrderItem> {
                new() { ProductId = 14, Quantity = 1, UnitPrice = 749.00m, TotalPrice = 749.00m },
                new() { ProductId = 16, Quantity = 1, UnitPrice = 139.00m, TotalPrice = 139.00m },
                new() { ProductId = 36, Quantity = 3, UnitPrice = 29.90m, TotalPrice = 89.70m }
            }
        },
        // Order 6: Processing (Sarah - 1 day ago)
        new() {
            OrderNumber = $"ORD-{DateTime.UtcNow.AddDays(-1):yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            UserId = users[0].Id,
            OrderDate = DateTime.UtcNow.AddDays(-1),
            SubTotal = 93.70m,
            ShippingCost = 5m,
            Discount = 0m,
            TotalAmount = 98.70m,
            CouponCode = "",
            Status = "Processing",
            PaymentMethod = "CreditCard",
            PaymentStatus = "Pending",
            Notes = "",
            ShippingAddressId = addresses[0].Id,
            BillingAddressId = addresses[0].Id,
            OrderItems = new List<OrderItem> {
                new() { ProductId = 20, Quantity = 1, UnitPrice = 39.90m, TotalPrice = 39.90m },
                new() { ProductId = 7, Quantity = 2, UnitPrice = 12.50m, TotalPrice = 25.00m },
                new() { ProductId = 31, Quantity = 1, UnitPrice = 89.90m, TotalPrice = 89.90m }
            }
        },
        // Order 7: Delivered (James - 1 month ago)
        new() {
            OrderNumber = $"ORD-{DateTime.UtcNow.AddMonths(-1):yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            UserId = users[1].Id,
            OrderDate = DateTime.UtcNow.AddMonths(-1),
            SubTotal = 82.60m,
            ShippingCost = 5m,
            Discount = 16.52m,
            TotalAmount = 71.08m,
            CouponCode = "BEANS20",
            Status = "Delivered",
            PaymentMethod = "CreditCard",
            PaymentStatus = "Paid",
            Notes = "",
            ShippingAddressId = addresses[1].Id,
            BillingAddressId = addresses[1].Id,
            ShippedAt = DateTime.UtcNow.AddMonths(-1).AddDays(1),
            DeliveredAt = DateTime.UtcNow.AddMonths(-1).AddDays(3),
            OrderItems = new List<OrderItem> {
                new() { ProductId = 9, Quantity = 1, UnitPrice = 32.90m, TotalPrice = 32.90m },
                new() { ProductId = 10, Quantity = 1, UnitPrice = 28.90m, TotalPrice = 28.90m },
                new() { ProductId = 39, Quantity = 1, UnitPrice = 8.90m, TotalPrice = 8.90m },
                new() { ProductId = 34, Quantity = 1, UnitPrice = 19.90m, TotalPrice = 19.90m }
            }
        },
        // Order 8: Cancelled (Emma - 1 week ago)
        new() {
            OrderNumber = $"ORD-{DateTime.UtcNow.AddDays(-7):yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            UserId = users[2].Id,
            OrderDate = DateTime.UtcNow.AddDays(-7),
            SubTotal = 449.00m,
            ShippingCost = 5m,
            Discount = 0m,
            TotalAmount = 454.00m,
            CouponCode = "",
            Status = "Cancelled",
            PaymentMethod = "BankTransfer",
            PaymentStatus = "Refunded",
            ShippingAddressId = addresses[2].Id,
            BillingAddressId = addresses[2].Id,
            Notes = "Customer changed mind",
            OrderItems = new List<OrderItem> {
                new() { ProductId = 12, Quantity = 1, UnitPrice = 449.00m, TotalPrice = 449.00m }
            }
        }
    };
            context.Orders.AddRange(orders);
            context.SaveChanges();

            // Create Invoices for Delivered and Shipped Orders
            var invoices = new List<Invoice>
    {
        new() { InvoiceNumber = $"INV-{DateTime.UtcNow.AddMonths(-3):yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}", OrderId = orders[0].Id, IssueDate = orders[0].OrderDate.AddDays(1), TotalAmount = orders[0].TotalAmount, PdfUrl = "" },
        new() { InvoiceNumber = $"INV-{DateTime.UtcNow.AddMonths(-2):yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}", OrderId = orders[1].Id, IssueDate = orders[1].OrderDate.AddDays(1), TotalAmount = orders[1].TotalAmount, PdfUrl = "" },
        new() { InvoiceNumber = $"INV-{DateTime.UtcNow.AddDays(-45):yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}", OrderId = orders[2].Id, IssueDate = orders[2].OrderDate.AddDays(1), TotalAmount = orders[2].TotalAmount, PdfUrl = "" },
        new() { InvoiceNumber = $"INV-{DateTime.UtcNow.AddDays(-5):yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}", OrderId = orders[3].Id, IssueDate = orders[3].OrderDate.AddDays(1), TotalAmount = orders[3].TotalAmount, PdfUrl = "" },
        new() { InvoiceNumber = $"INV-{DateTime.UtcNow.AddMonths(-1):yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}", OrderId = orders[6].Id, IssueDate = orders[6].OrderDate.AddDays(1), TotalAmount = orders[6].TotalAmount, PdfUrl = "" }
    };
            context.Invoices.AddRange(invoices);
            context.SaveChanges();

            // Create Reviews for delivered products
            var reviews = new List<Review>
    {
        new() { UserId = users[0].Id, ProductId = 1, Rating = 5, Title = "Excellent Ethiopian coffee!", Comment = "Floral and fruity notes are amazing. Perfect for V60 brewing. Highly recommend!", CreatedAt = DateTime.UtcNow.AddMonths(-3).AddDays(10) },
        new() { UserId = users[0].Id, ProductId = 3, Rating = 4, Title = "Great for espresso", Comment = "Smooth and chocolatey. Works perfectly in my espresso machine.", CreatedAt = DateTime.UtcNow.AddMonths(-3).AddDays(8) },
        new() { UserId = users[1].Id, ProductId = 11, Rating = 5, Title = "Best home espresso machine!", Comment = "The built-in grinder is fantastic. Makes cafe-quality espresso at home.", CreatedAt = DateTime.UtcNow.AddMonths(-2).AddDays(5) },
        new() { UserId = users[1].Id, ProductId = 16, Rating = 5, Title = "Perfect grinder for beginners", Comment = "Easy to use with great consistency. 40 grind settings cover all brewing methods.", CreatedAt = DateTime.UtcNow.AddMonths(-2).AddDays(4) },
        new() { UserId = users[2].Id, ProductId = 21, Rating = 5, Title = "Beautiful and functional", Comment = "The Chemex looks great and makes amazing coffee. Love the clean taste.", CreatedAt = DateTime.UtcNow.AddDays(-40) },
        new() { UserId = users[2].Id, ProductId = 26, Rating = 4, Title = "Accurate and reliable", Comment = "The timer function is very convenient. Display is easy to read.", CreatedAt = DateTime.UtcNow.AddDays(-40) },
        new() { UserId = users[1].Id, ProductId = 9, Rating = 4, Title = "Great espresso blend", Comment = "Perfect crema and balanced flavor. My go-to beans for espresso.", CreatedAt = DateTime.UtcNow.AddMonths(-1).AddDays(5) },
        new() { UserId = users[1].Id, ProductId = 10, Rating = 5, Title = "Versatile house blend", Comment = "Works great for both espresso and filter coffee. Smooth and not too acidic.", CreatedAt = DateTime.UtcNow.AddMonths(-1).AddDays(5) },
        new() { UserId = users[0].Id, ProductId = 20, Rating = 5, Title = "Love my AeroPress!", Comment = "Quick, easy, and makes delicious coffee. Perfect for travel too.", CreatedAt = DateTime.UtcNow.AddDays(-35) },
        new() { UserId = users[3].Id, ProductId = 17, Rating = 5, Title = "Premium hand grinder", Comment = "Built like a tank. Grind consistency is incredible for manual grinder.", CreatedAt = DateTime.UtcNow.AddDays(-30) },
        new() { UserId = users[2].Id, ProductId = 2, Rating = 4, Title = "Solid Colombian beans", Comment = "Nice balanced flavor with sweet notes. Good value for money.", CreatedAt = DateTime.UtcNow.AddDays(-38) }
    };
            context.Reviews.AddRange(reviews);
            context.SaveChanges();

            tx.Commit();
        }
    }
}