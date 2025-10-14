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

            builder.Services.AddScoped<IFileUploadService>(provider =>
            {
                var env = provider.GetRequiredService<IWebHostEnvironment>();
                return new FileUploadService(env.WebRootPath);
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

            var categories = new List<Category>
    {
        new() { Name = "Espresso", Description = "Strong, concentrated espresso-based coffees" },
        new() { Name = "Cappuccino", Description = "Espresso with steamed milk foam" },
        new() { Name = "Latte", Description = "Milk coffees with smooth flavor" },
        new() { Name = "Specialty", Description = "Special coffee creations" },
        new() { Name = "Cold Brew", Description = "Cold brewed coffees" },
        new() { Name = "Coffee Beans", Description = "Premium coffee beans from around the world" },
        new() { Name = "Pastries", Description = "Freshly baked pastries and desserts" },
        new() { Name = "Sandwiches", Description = "Fresh sandwiches and snacks" }
    };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            var products = new List<Product>
    {
        // Espresso
        new() { Name = "Espresso", Description = "Classic Italian espresso", Price = 2.50m, StockQuantity = 100, ImageUrl = "/images/espresso.jpg", RoastLevel="Medium-Dark", Origin="Italy", CategoryId = 1 },
        new() { Name = "Double Espresso", Description = "Double shot of espresso", Price = 3.50m, StockQuantity = 100, ImageUrl = "/images/double-espresso.jpg", RoastLevel="Dark", Origin="Italy", CategoryId = 1 },
        new() { Name = "Americano", Description = "Espresso with hot water", Price = 2.90m, StockQuantity = 100, ImageUrl = "/images/americano.jpg", RoastLevel="Medium", Origin="Brazil", CategoryId = 1 },
        
        // Cappuccino
        new() { Name = "Cappuccino", Description = "Espresso with milk foam", Price = 3.50m, StockQuantity = 100, ImageUrl = "/images/cappuccino.jpg", RoastLevel="Medium", Origin="Italy", CategoryId = 2 },
        new() { Name = "Vanilla Cappuccino", Description = "Vanilla cappuccino", Price = 3.90m, StockQuantity = 100, ImageUrl = "/images/vanilla-cappuccino.jpg", RoastLevel="Medium", Origin="Colombia", CategoryId = 2 },
        new() { Name = "Caramel Cappuccino", Description = "Caramel cappuccino", Price = 3.90m, StockQuantity = 100, ImageUrl = "/images/caramel-cappuccino.jpg", RoastLevel="Medium", Origin="Colombia", CategoryId = 2 },
        
        // Latte
        new() { Name = "Caffè Latte", Description = "Classic milk coffee", Price = 3.90m, StockQuantity = 100, ImageUrl = "/images/latte.jpg", RoastLevel="Light-Medium", Origin="Brazil", CategoryId = 3 },
        new() { Name = "Vanilla Latte", Description = "Vanilla latte", Price = 4.20m, StockQuantity = 100, ImageUrl = "/images/vanilla-latte.jpg", RoastLevel="Light", Origin="Colombia", CategoryId = 3 },
        new() { Name = "Mocha Latte", Description = "Chocolate latte", Price = 4.50m, StockQuantity = 100, ImageUrl = "/images/mocha-latte.jpg", RoastLevel="Light", Origin="Peru", CategoryId = 3 },
        new() { Name = "Almond Latte", Description = "Almond flavored latte", Price = 4.20m, StockQuantity = 100, ImageUrl = "/images/almond-latte.jpg", RoastLevel="Light", Origin="Guatemala", CategoryId = 3 },
        
        // Specialty
        new() { Name = "Flat White", Description = "Australian specialty coffee", Price = 4.20m, StockQuantity = 100, ImageUrl = "/images/flat-white.jpg", RoastLevel="Medium", Origin="Australia", CategoryId = 4 },
        new() { Name = "Macchiato", Description = "Espresso with milk foam", Price = 3.20m, StockQuantity = 100, ImageUrl = "/images/macchiato.jpg", RoastLevel="Medium-Dark", Origin="Italy", CategoryId = 4 },
        new() { Name = "Affogato", Description = "Espresso with vanilla ice cream", Price = 5.20m, StockQuantity = 50, ImageUrl = "/images/affogato.jpg", RoastLevel="Dark", Origin="Italy", CategoryId = 4 },
        new() { Name = "Irish Coffee", Description = "Coffee with whiskey and cream", Price = 5.90m, StockQuantity = 50, ImageUrl = "/images/irish-coffee.jpg", RoastLevel="Dark", Origin="Ireland", CategoryId = 4 },
        
        // Cold Brew
        new() { Name = "Cold Brew", Description = "Cold brewed coffee", Price = 4.20m, StockQuantity = 80, ImageUrl = "/images/cold-brew.jpg", RoastLevel="Medium", Origin="Kenya", CategoryId = 5 },
        new() { Name = "Iced Latte", Description = "Iced latte", Price = 4.50m, StockQuantity = 80, ImageUrl = "/images/iced-latte.jpg", RoastLevel="Light", Origin="Colombia", CategoryId = 5 },
        new() { Name = "Frappuccino", Description = "Iced blended coffee", Price = 5.20m, StockQuantity = 80, ImageUrl = "/images/frappuccino.jpg", RoastLevel="Medium", Origin="Colombia", CategoryId = 5 },
        
        // Coffee Beans - PREMIUM
        new() { Name = "Ethiopian Yirgacheffe", Description = "Floral and citrus notes with bright acidity. Perfect for pour-over.", Price = 14.90m, StockQuantity = 50, ImageUrl = "/images/beans-ethiopian.jpg", RoastLevel="Light", Origin="Ethiopia", IsOrganic=true, CategoryId = 6 },
        new() { Name = "Colombian Supremo", Description = "Balanced body with caramel sweetness and nutty undertones.", Price = 12.90m, StockQuantity = 60, ImageUrl = "/images/beans-colombian.jpg", RoastLevel="Medium", Origin="Colombia", CategoryId = 6 },
        new() { Name = "Brazilian Santos", Description = "Smooth, low-acidity with chocolate notes. Great for espresso.", Price = 11.90m, StockQuantity = 70, ImageUrl = "/images/beans-brazilian.jpg", RoastLevel="Medium-Dark", Origin="Brazil", CategoryId = 6 },
        new() { Name = "Guatemalan Antigua", Description = "Rich, full-bodied with spicy and smoky notes.", Price = 13.90m, StockQuantity = 45, ImageUrl = "/images/beans-guatemalan.jpg", RoastLevel="Dark", Origin="Guatemala", IsOrganic=true, CategoryId = 6 },
        new() { Name = "Kenyan AA", Description = "Bold, wine-like acidity with berry and black currant flavors.", Price = 16.90m, StockQuantity = 40, ImageUrl = "/images/beans-kenyan.jpg", RoastLevel="Medium", Origin="Kenya", CategoryId = 6 },
        new() { Name = "Costa Rican Tarrazu", Description = "Clean, crisp with bright citrus acidity.", Price = 13.50m, StockQuantity = 55, ImageUrl = "/images/beans-costarican.jpg", RoastLevel="Light-Medium", Origin="Costa Rica", IsOrganic=true, CategoryId = 6 },
        new() { Name = "Sumatra Mandheling", Description = "Earthy, herbal with low acidity and full body.", Price = 12.50m, StockQuantity = 50, ImageUrl = "/images/beans-sumatra.jpg", RoastLevel="Dark", Origin="Indonesia", CategoryId = 6 },
        new() { Name = "Decaf Swiss Water", Description = "Smooth decaf with no chemical residue. Medium roast blend.", Price = 11.90m, StockQuantity = 35, ImageUrl = "/images/beans-decaf.jpg", RoastLevel="Medium", Origin="Colombia", IsCaffeineFree=true, CategoryId = 6 },
        new() { Name = "Espresso Blend", Description = "Rich, balanced blend perfect for espresso machines.", Price = 10.90m, StockQuantity = 80, ImageUrl = "/images/beans-espresso.jpg", RoastLevel="Dark", Origin="Blend", CategoryId = 6 },
        new() { Name = "House Blend", Description = "Our signature blend - smooth and versatile.", Price = 8.90m, StockQuantity = 100, ImageUrl = "/images/beans-house.jpg", RoastLevel="Medium", Origin="Blend", CategoryId = 6 },
        
        // Pastries
        new() { Name = "Croissant", Description = "Butter croissant", Price = 2.90m, StockQuantity = 40, ImageUrl = "/images/croissant.jpg", RoastLevel="N/A", Origin="France", CategoryId = 7 },
        new() { Name = "Chocolate Muffin", Description = "Chocolate muffin", Price = 3.20m, StockQuantity = 30, ImageUrl = "/images/choco-muffin.jpg", RoastLevel="N/A", Origin="USA", CategoryId = 7 },
        new() { Name = "Blueberry Muffin", Description = "Blueberry muffin", Price = 3.20m, StockQuantity = 30, ImageUrl = "/images/blueberry-muffin.jpg", RoastLevel="N/A", Origin="USA", CategoryId = 7 },
        new() { Name = "Brownie", Description = "Chocolate brownie", Price = 3.50m, StockQuantity = 25, ImageUrl = "/images/brownie.jpg", RoastLevel="N/A", Origin="USA", CategoryId = 7 },
        new() { Name = "Cheesecake", Description = "New York cheesecake", Price = 5.20m, StockQuantity = 20, ImageUrl = "/images/cheesecake.jpg", RoastLevel="N/A", Origin="USA", CategoryId = 7 },
        
        // Sandwiches
        new() { Name = "Club Sandwich", Description = "Chicken, bacon, lettuce, tomato", Price = 6.90m, StockQuantity = 30, ImageUrl = "/images/club-sandwich.jpg", RoastLevel="N/A", Origin="USA", CategoryId = 8 },
        new() { Name = "Tuna Sandwich", Description = "Tuna, lettuce, onion", Price = 5.90m, StockQuantity = 30, ImageUrl = "/images/tuna-sandwich.jpg", RoastLevel="N/A", Origin="Mediterranean", CategoryId = 8 },
        new() { Name = "Vegan Wrap", Description = "Tortilla with vegetables", Price = 5.50m, StockQuantity = 25, ImageUrl = "/images/vegan-wrap.jpg", RoastLevel="N/A", Origin="Mexico", CategoryId = 8 },
        new() { Name = "Mozzarella Panini", Description = "Mozzarella, tomato, basil", Price = 6.20m, StockQuantity = 25, ImageUrl = "/images/panini.jpg", RoastLevel="N/A", Origin="Italy", CategoryId = 8 }
    };
            context.Products.AddRange(products);
            context.SaveChanges();

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

            var users = new List<User>
    {
        new() { Name="John Smith", Email="john.smith@example.com", PasswordHash=Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("Password123"))), Role="RegisteredUser", EmailConfirmed=true },
        new() { Name="Anna Johnson", Email="anna.johnson@example.com", PasswordHash=Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("Password123"))), Role="RegisteredUser", EmailConfirmed=true },
        new() { Name="Peter Williams", Email="peter.williams@example.com", PasswordHash=Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("Password123"))), Role="RegisteredUser", EmailConfirmed=true }
    };
            context.Users.AddRange(users);
            context.SaveChanges();

            var addresses = users.Select(u => new Address
            {
                FirstName = u.Name.Split(' ').First(),
                LastName = u.Name.Split(' ').Last(),
                AddressLine1 = "Main Street 1",
                City = "Berlin",
                PostalCode = "10115",
                Country = "Germany",
                PhoneNumber = "+49 30 12345678",
                IsDefault = true,
                AddressType = "Shipping",
                UserId = u.Id
            }).ToList();
            context.Addresses.AddRange(addresses);
            context.SaveChanges();

            var coupons = new List<Coupon>
    {
        new() { Code = "WELCOME10", Description = "10% discount for new customers", DiscountType = "Percentage", DiscountValue = 10m, MinimumOrderAmount = 15m, StartDate = DateTime.UtcNow.AddDays(-30), EndDate = DateTime.UtcNow.AddDays(30), IsActive = true },
        new() { Code = "SUMMER2025", Description = "Summer sale - 5€ off", DiscountType = "FixedAmount", DiscountValue = 5m, MinimumOrderAmount = 25m, StartDate = DateTime.UtcNow.AddDays(-10), EndDate = DateTime.UtcNow.AddDays(60), IsActive = true },
        new() { Code = "FREESHIP", Description = "Free shipping over 30€", DiscountType = "FixedAmount", DiscountValue = 5m, MinimumOrderAmount = 30m, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(90), IsActive = true }
    };
            context.Coupons.AddRange(coupons);
            context.SaveChanges();

            tx.Commit();
        }
    }
}