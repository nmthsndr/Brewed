
namespace Brewed
{
    using System;
    using Brewed.DataContext.Context;
    using Brewed.Services;
    using Brewed.DataContext.Entities;
    using Microsoft.CodeAnalysis.Scripting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.OpenApi.Models;

    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<BrewedDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add services to the container.
            builder.Services.AddControllers();

            // Swagger setup
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Brewer API",
                    Version = "v1"
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Brewer API v1");
                });
            }

            app.UseHttpsRedirection();

            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<BrewedDbContext>();
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
            // ha már vannak kategóriák, feltételezzük, hogy le van magozva
            if (context.Categories.Any())
                return;

            using var tx = context.Database.BeginTransaction();

            // ----- KATEGÓRIÁK -----
            var categories = new List<Category>
        {
            new() { Name = "Espresso",     Description = "Erős, koncentrált kávék espresso alapon" },
            new() { Name = "Cappuccino",   Description = "Tejhabos kávékülönlegességek" },
            new() { Name = "Latte",        Description = "Tejes kávék lágy ízvilággal" },
            new() { Name = "Specialty",    Description = "Különleges kávékreációk" },
            new() { Name = "Cold Brew",    Description = "Hideg főzésű kávék" },
            new() { Name = "Sütemények",   Description = "Frissen sült péksütik és desszertek" },
            new() { Name = "Szendvicsek",  Description = "Friss szendvicsek és snackek" }
        };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            // ----- TERMÉKEK -----
            var products = new List<Product>
        {
            // Espresso
            new() { Name = "Espresso", Description = "Klasszikus olasz espresso", Price = 590m,  StockQuantity = 100, ImageUrl = "/images/espresso.jpg",        RoastLevel="Medium-Dark", Origin="Blend", Category = categories[0] },
            new() { Name = "Dupla Espresso", Description = "Dupla adag espresso", Price = 890m, StockQuantity = 100, ImageUrl = "/images/double-espresso.jpg", RoastLevel="Dark", Origin="Blend", Category = categories[0] },
            new() { Name = "Americano", Description = "Espresso forró vízzel", Price = 690m,   StockQuantity = 100, ImageUrl = "/images/americano.jpg",        RoastLevel="Medium", Origin="Blend", Category = categories[0] },

            // Cappuccino
            new() { Name = "Cappuccino", Description = "Espresso tejes habbal", Price = 890m,  StockQuantity = 100, ImageUrl = "/images/cappuccino.jpg",          RoastLevel="Medium", Origin="Blend", Category = categories[1] },
            new() { Name = "Vanília Cappuccino", Description = "Vaníliás cappuccino", Price = 990m, StockQuantity = 100, ImageUrl = "/images/vanilla-cappuccino.jpg", RoastLevel="Medium", Origin="Blend", Category = categories[1] },
            new() { Name = "Karamell Cappuccino", Description = "Karamellás cappuccino", Price = 990m, StockQuantity = 100, ImageUrl = "/images/caramel-cappuccino.jpg", RoastLevel="Medium", Origin="Blend", Category = categories[1] },

            // Latte
            new() { Name = "Caffè Latte", Description = "Klasszikus tejeskávé", Price = 990m, StockQuantity = 100, ImageUrl = "/images/latte.jpg",            RoastLevel="Light-Medium", Origin="Brazil", Category = categories[2] },
            new() { Name = "Vanília Latte", Description = "Vaníliás latte", Price = 1090m,   StockQuantity = 100, ImageUrl = "/images/vanilla-latte.jpg",    RoastLevel="Light", Origin="Colombia", Category = categories[2] },
            new() { Name = "Mocha Latte", Description = "Csokoládés latte", Price = 1190m,  StockQuantity = 100, ImageUrl = "/images/mocha-latte.jpg",       RoastLevel="Light", Origin="Peru", Category = categories[2] },
            new() { Name = "Mandulás Latte", Description = "Mandula ízesítésű latte", Price = 1090m, StockQuantity = 100, ImageUrl = "/images/almond-latte.jpg", RoastLevel="Light", Origin="Guatemala", Category = categories[2] },

            // Specialty
            new() { Name = "Flat White", Description = "Ausztrál specialty kávé", Price = 1090m, StockQuantity = 100, ImageUrl = "/images/flat-white.jpg", RoastLevel="Medium", Origin="Australia", Category = categories[3] },
            new() { Name = "Macchiato", Description = "Espresso tejhabbal", Price = 790m, StockQuantity = 100, ImageUrl = "/images/macchiato.jpg", RoastLevel="Medium-Dark", Origin="Italy", Category = categories[3] },
            new() { Name = "Affogato", Description = "Espresso vaníliafagylalttal", Price = 1290m, StockQuantity = 50, ImageUrl = "/images/affogato.jpg", RoastLevel="Dark", Origin="Blend", Category = categories[3] },
            new() { Name = "Irish Coffee", Description = "Kávé whiskyvel és tejszínnel", Price = 1490m, StockQuantity = 50, ImageUrl = "/images/irish-coffee.jpg", RoastLevel="Dark", Origin="Ireland", Category = categories[3] },

            // Cold Brew
            new() { Name = "Cold Brew", Description = "Hideg főzésű kávé", Price = 1090m, StockQuantity = 80, ImageUrl = "/images/cold-brew.jpg", RoastLevel="Medium", Origin="Kenya", Category = categories[4] },
            new() { Name = "Iced Latte", Description = "Jeges latte", Price = 1190m, StockQuantity = 80, ImageUrl = "/images/iced-latte.jpg", RoastLevel="Light", Origin="Colombia", Category = categories[4] },
            new() { Name = "Frappuccino", Description = "Jeges turmix kávé", Price = 1390m, StockQuantity = 80, ImageUrl = "/images/frappuccino.jpg", RoastLevel="Medium", Origin="Blend", Category = categories[4] },

            // Sütemények
            new() { Name = "Croissant", Description = "Vajas croissant", Price = 690m, StockQuantity = 40, ImageUrl = "/images/croissant.jpg", Category = categories[5] },
            new() { Name = "Csokis Muffin", Description = "Csokoládés muffin", Price = 790m, StockQuantity = 30, ImageUrl = "/images/choco-muffin.jpg", Category = categories[5] },
            new() { Name = "Áfonyás Muffin", Description = "Áfonyás muffin", Price = 790m, StockQuantity = 30, ImageUrl = "/images/blueberry-muffin.jpg", Category = categories[5] },
            new() { Name = "Brownie", Description = "Csokoládés brownie", Price = 890m, StockQuantity = 25, ImageUrl = "/images/brownie.jpg", Category = categories[5] },
            new() { Name = "Sajttorta", Description = "New York-i sajttorta", Price = 1290m, StockQuantity = 20, ImageUrl = "/images/cheesecake.jpg", Category = categories[5] },

            // Szendvicsek
            new() { Name = "Club Sandwich", Description = "Csirke, bacon, saláta, paradicsom", Price = 1590m, StockQuantity = 30, ImageUrl = "/images/club-sandwich.jpg", Category = categories[6] },
            new() { Name = "Tonhalas Szendvics", Description = "Tonhal, saláta, hagyma", Price = 1390m, StockQuantity = 30, ImageUrl = "/images/tuna-sandwich.jpg", Category = categories[6] },
            new() { Name = "Vegán Wrap", Description = "Zöldségekkel töltött tortilla", Price = 1290m, StockQuantity = 25, ImageUrl = "/images/vegan-wrap.jpg", Category = categories[6] },
            new() { Name = "Mozzarella Panini", Description = "Mozzarella, paradicsom, bazsalikom", Price = 1490m, StockQuantity = 25, ImageUrl = "/images/panini.jpg", Category = categories[6] }
        };
            context.Products.AddRange(products);
            context.SaveChanges();

            // ----- FELHASZNÁLÓK + CÍMEK -----
            var users = new List<User>
        {
            new() { Name="Nagy János",  Email="nagy.janos@example.com",  PasswordHash="hashedpassword123", Role="RegisteredUser" },
            new() { Name="Kovács Anna", Email="kovacs.anna@example.com", PasswordHash="hashedpassword123", Role="RegisteredUser" },
            new() { Name="Szabó Péter",  Email="szabo.peter@example.com",  PasswordHash="hashedpassword123", Role="RegisteredUser" },
            new() { Name="Tóth Eszter",  Email="toth.eszter@example.com",  PasswordHash="hashedpassword123", Role="RegisteredUser" },
            new() { Name="Molnár Gábor", Email="molnar.gabor@example.com", PasswordHash="hashedpassword123", Role="RegisteredUser" }
        };
            context.Users.AddRange(users);
            context.SaveChanges();

            // minden usernek egy shipping cím
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

            // ----- RENDELÉSEK + TÉTELEK -----
            var rnd = new Random();
            var orders = new List<Order>();
            var orderItems = new List<OrderItem>();

            foreach (var u in users)
            {
                int orderCount = rnd.Next(1, 4);
                var shipAddrId = addresses.First(a => a.UserId == u.Id).Id;

                for (int i = 0; i < orderCount; i++)
                {
                    var created = DateTime.UtcNow.AddDays(-rnd.Next(1, 30));
                    var order = new Order
                    {
                        UserId = u.Id,
                        ShippingAddressId = shipAddrId,
                        BillingAddressId = null,
                        OrderNumber = $"ORD-{u.Id:D3}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                        OrderDate = created,
                        Status = "Processing",
                        PaymentMethod = "Card",
                        PaymentStatus = "Pending",
                        CouponCode = string.Empty,
                        Notes = string.Empty,
                        SubTotal = 0m,
                        ShippingCost = 0m,
                        Discount = 0m,
                        TotalAmount = 0m
                    };
                    orders.Add(order);
                }
            }

            context.Orders.AddRange(orders);
            context.SaveChanges();

            foreach (var o in orders)
            {
                var itemCount = rnd.Next(1, 4);
                var picked = products.OrderBy(_ => rnd.Next()).Take(itemCount).ToList();

                decimal sub = 0m;
                foreach (var p in picked)
                {
                    var qty = rnd.Next(1, 3);
                    var unit = p.Price;
                    var line = new OrderItem
                    {
                        OrderId = o.Id,
                        ProductId = p.Id,
                        Quantity = qty,
                        UnitPrice = unit,
                        TotalPrice = unit * qty
                    };
                    sub += line.TotalPrice;
                    orderItems.Add(line);
                }

                o.SubTotal = sub;
                o.ShippingCost = sub >= 5000m ? 0m : 690m;
                o.Discount = 0m;
                o.TotalAmount = o.SubTotal + o.ShippingCost - o.Discount;
            }

            context.OrderItems.AddRange(orderItems);
            context.Orders.UpdateRange(orders);
            context.SaveChanges();

            tx.Commit();
        }
    }
}
