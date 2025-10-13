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

            // Services - MINDEN SERVICE REGISZTRÁLÁSA
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

            // FileUploadService - Special registration with webRootPath
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

            // CORSPolicy
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

            // Swagger with JWT support
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Brewed Coffee API",
                    Version = "v1",
                    Description = "Kávé webshop API - Teljes dokumentáció"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header. Írd be: {token}"
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

            app.UseCors("AllowAll");  // ← ELSŐ helyre!
            // app.UseHttpsRedirection();  // ← Ki van commentelve
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

                    // Apply migrations
                    context.Database.Migrate();

                    // Seed data
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

            // KATEGÓRIÁK
            var categories = new List<Category>
            {
                new() { Name = "Espresso", Description = "Erős, koncentrált kávék espresso alapon" },
                new() { Name = "Cappuccino", Description = "Tejhabos kávékülönlegességek" },
                new() { Name = "Latte", Description = "Tejes kávék lágy ízvilággal" },
                new() { Name = "Specialty", Description = "Különleges kávékreációk" },
                new() { Name = "Cold Brew", Description = "Hideg főzésű kávék" },
                new() { Name = "Sütemények", Description = "Frissen sült péksütik és desszertek" },
                new() { Name = "Szendvicsek", Description = "Friss szendvicsek és snackek" }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            // TERMÉKEK - CategoryId használatával!
            var products = new List<Product>
            {
                new() { Name = "Espresso", Description = "Klasszikus olasz espresso", Price = 590m, StockQuantity = 100, ImageUrl = "/images/espresso.jpg", RoastLevel="Medium-Dark", Origin="Blend", CategoryId = 1 },
                new() { Name = "Dupla Espresso", Description = "Dupla adag espresso", Price = 890m, StockQuantity = 100, ImageUrl = "/images/double-espresso.jpg", RoastLevel="Dark", Origin="Blend", CategoryId = 1 },
                new() { Name = "Americano", Description = "Espresso forró vízzel", Price = 690m, StockQuantity = 100, ImageUrl = "/images/americano.jpg", RoastLevel="Medium", Origin="Blend", CategoryId = 1 },
                new() { Name = "Cappuccino", Description = "Espresso tejes habbal", Price = 890m, StockQuantity = 100, ImageUrl = "/images/cappuccino.jpg", RoastLevel="Medium", Origin="Blend", CategoryId = 2 },
                new() { Name = "Vanília Cappuccino", Description = "Vaníliás cappuccino", Price = 990m, StockQuantity = 100, ImageUrl = "/images/vanilla-cappuccino.jpg", RoastLevel="Medium", Origin="Blend", CategoryId = 2 },
                new() { Name = "Karamell Cappuccino", Description = "Karamellás cappuccino", Price = 990m, StockQuantity = 100, ImageUrl = "/images/caramel-cappuccino.jpg", RoastLevel="Medium", Origin="Blend", CategoryId = 2 },
                new() { Name = "Caffè Latte", Description = "Klasszikus tejeskávé", Price = 990m, StockQuantity = 100, ImageUrl = "/images/latte.jpg", RoastLevel="Light-Medium", Origin="Brazil", CategoryId = 3 },
                new() { Name = "Vanília Latte", Description = "Vaníliás latte", Price = 1090m, StockQuantity = 100, ImageUrl = "/images/vanilla-latte.jpg", RoastLevel="Light", Origin="Colombia", CategoryId = 3 },
                new() { Name = "Mocha Latte", Description = "Csokoládés latte", Price = 1190m, StockQuantity = 100, ImageUrl = "/images/mocha-latte.jpg", RoastLevel="Light", Origin="Peru", CategoryId = 3 },
                new() { Name = "Mandulás Latte", Description = "Mandula ízesítésű latte", Price = 1090m, StockQuantity = 100, ImageUrl = "/images/almond-latte.jpg", RoastLevel="Light", Origin="Guatemala", CategoryId = 3 },
                new() { Name = "Flat White", Description = "Ausztrál specialty kávé", Price = 1090m, StockQuantity = 100, ImageUrl = "/images/flat-white.jpg", RoastLevel="Medium", Origin="Australia", CategoryId = 4 },
                new() { Name = "Macchiato", Description = "Espresso tejhabbal", Price = 790m, StockQuantity = 100, ImageUrl = "/images/macchiato.jpg", RoastLevel="Medium-Dark", Origin="Italy", CategoryId = 4 },
                new() { Name = "Affogato", Description = "Espresso vaníliafagylalttal", Price = 1290m, StockQuantity = 50, ImageUrl = "/images/affogato.jpg", RoastLevel="Dark", Origin="Blend", CategoryId = 4 },
                new() { Name = "Irish Coffee", Description = "Kávé whiskyvel és tejszínnel", Price = 1490m, StockQuantity = 50, ImageUrl = "/images/irish-coffee.jpg", RoastLevel="Dark", Origin="Ireland", CategoryId = 4 },
                new() { Name = "Cold Brew", Description = "Hideg főzésű kávé", Price = 1090m, StockQuantity = 80, ImageUrl = "/images/cold-brew.jpg", RoastLevel="Medium", Origin="Kenya", CategoryId = 5 },
                new() { Name = "Iced Latte", Description = "Jeges latte", Price = 1190m, StockQuantity = 80, ImageUrl = "/images/iced-latte.jpg", RoastLevel="Light", Origin="Colombia", CategoryId = 5 },
                new() { Name = "Frappuccino", Description = "Jeges turmix kávé", Price = 1390m, StockQuantity = 80, ImageUrl = "/images/frappuccino.jpg", RoastLevel="Medium", Origin="Blend", CategoryId = 5 },
                new() { Name = "Croissant", Description = "Vajas croissant", Price = 690m, StockQuantity = 40, ImageUrl = "/images/croissant.jpg", CategoryId = 6 },
                new() { Name = "Csokis Muffin", Description = "Csokoládés muffin", Price = 790m, StockQuantity = 30, ImageUrl = "/images/choco-muffin.jpg", CategoryId = 6 },
                new() { Name = "Áfonyás Muffin", Description = "Áfonyás muffin", Price = 790m, StockQuantity = 30, ImageUrl = "/images/blueberry-muffin.jpg", CategoryId = 6 },
                new() { Name = "Brownie", Description = "Csokoládés brownie", Price = 890m, StockQuantity = 25, ImageUrl = "/images/brownie.jpg", CategoryId = 6 },
                new() { Name = "Sajttorta", Description = "New York-i sajttorta", Price = 1290m, StockQuantity = 20, ImageUrl = "/images/cheesecake.jpg", CategoryId = 6 },
                new() { Name = "Club Sandwich", Description = "Csirke, bacon, saláta, paradicsom", Price = 1590m, StockQuantity = 30, ImageUrl = "/images/club-sandwich.jpg", CategoryId = 7 },
                new() { Name = "Tonhalas Szendvics", Description = "Tonhal, saláta, hagyma", Price = 1390m, StockQuantity = 30, ImageUrl = "/images/tuna-sandwich.jpg", CategoryId = 7 },
                new() { Name = "Vegán Wrap", Description = "Zöldségekkel töltött tortilla", Price = 1290m, StockQuantity = 25, ImageUrl = "/images/vegan-wrap.jpg", CategoryId = 7 },
                new() { Name = "Mozzarella Panini", Description = "Mozzarella, paradicsom, bazsalikom", Price = 1490m, StockQuantity = 25, ImageUrl = "/images/panini.jpg", CategoryId = 7 }
            };
            context.Products.AddRange(products);
            context.SaveChanges();

            // ADMIN USER
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

            // DEMO USERS
            var users = new List<User>
            {
                new() { Name="Nagy János", Email="nagy.janos@example.com", PasswordHash=Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("Password123"))), Role="RegisteredUser", EmailConfirmed=true },
                new() { Name="Kovács Anna", Email="kovacs.anna@example.com", PasswordHash=Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("Password123"))), Role="RegisteredUser", EmailConfirmed=true },
                new() { Name="Szabó Péter", Email="szabo.peter@example.com", PasswordHash=Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("Password123"))), Role="RegisteredUser", EmailConfirmed=true }
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // CÍMEK
            var addresses = users.Select(u => new Address
            {
                FirstName = u.Name.Split(' ').Last(),
                LastName = u.Name.Split(' ').First(),
                AddressLine1 = "Fő utca 1.",
                City = "Budapest",
                PostalCode = "1011",
                Country = "Hungary",
                PhoneNumber = "+36 1 234 5678",
                IsDefault = true,
                AddressType = "Shipping",
                UserId = u.Id
            }).ToList();
            context.Addresses.AddRange(addresses);
            context.SaveChanges();

            // KUPONOK
            var coupons = new List<Coupon>
            {
                new() { Code = "WELCOME10", Description = "10% kedvezmény új vásárlóknak", DiscountType = "Percentage", DiscountValue = 10m, MinimumOrderAmount = 3000m, StartDate = DateTime.UtcNow.AddDays(-30), EndDate = DateTime.UtcNow.AddDays(30), IsActive = true },
                new() { Code = "SUMMER2025", Description = "Nyári akció - 500 Ft kedvezmény", DiscountType = "FixedAmount", DiscountValue = 500m, MinimumOrderAmount = 5000m, StartDate = DateTime.UtcNow.AddDays(-10), EndDate = DateTime.UtcNow.AddDays(60), IsActive = true },
                new() { Code = "FREESHIP", Description = "Ingyenes szállítás 10000 Ft felett", DiscountType = "FixedAmount", DiscountValue = 1000m, MinimumOrderAmount = 10000m, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(90), IsActive = true }
            };
            context.Coupons.AddRange(coupons);
            context.SaveChanges();

            tx.Commit();
        }
    }
}