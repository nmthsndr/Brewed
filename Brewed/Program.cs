
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

            app.Run();

        }
        void SeedData(BrewedDbContext context)
        {
            if (!context.Categories.Any())
            {
                var categories = new List<Brewed.DataContext.Entities.Category>
        {
            new() { Name = "Espresso", Description = "Erõs, koncentrált kávék espresso alapon" },
            new() { Name = "Cappuccino", Description = "Tejhabos kávékülönlegességek" },
            new() { Name = "Latte", Description = "Tejes kávék lágy ízvilággal" },
            new() { Name = "Specialty", Description = "Különleges kávékreációk" },
            new() { Name = "Cold Brew", Description = "Hideg fõzésû kávék" },
            new() { Name = "Sütemények", Description = "Frissen sült péksütik és desszertek" },
            new() { Name = "Szendvicsek", Description = "Friss szendvicsek és snackek" }
        };

                context.Categories.AddRange(categories);
                context.SaveChanges();

                var products = new List<Brewed.DataContext.Entities.Product>
        {
            // Espresso kategória
            new() { Name = "Espresso", Description = "Klasszikus olasz espresso", Price = 590m, Stock = 100, ImageUrl = "/images/espresso.jpg", CategoryId = categories[0].Id },
            new() { Name = "Dupla Espresso", Description = "Dupla adag espresso", Price = 890m, Stock = 100, ImageUrl = "/images/double-espresso.jpg", CategoryId = categories[0].Id },
            new() { Name = "Americano", Description = "Espresso forró vízzel", Price = 690m, Stock = 100, ImageUrl = "/images/americano.jpg", CategoryId = categories[0].Id },
            
            // Cappuccino kategória
            new() { Name = "Cappuccino", Description = "Espresso tejes habbal", Price = 890m, Stock = 100, ImageUrl = "/images/cappuccino.jpg", CategoryId = categories[1].Id },
            new() { Name = "Vanília Cappuccino", Description = "Vaníliás cappuccino", Price = 990m, Stock = 100, ImageUrl = "/images/vanilla-cappuccino.jpg", CategoryId = categories[1].Id },
            new() { Name = "Karamell Cappuccino", Description = "Karamellás cappuccino", Price = 990m, Stock = 100, ImageUrl = "/images/caramel-cappuccino.jpg", CategoryId = categories[1].Id },
            
            // Latte kategória
            new() { Name = "Caff? Latte", Description = "Klasszikus tejeskávé", Price = 990m, Stock = 100, ImageUrl = "/images/latte.jpg", CategoryId = categories[2].Id },
            new() { Name = "Vanília Latte", Description = "Vaníliás latte", Price = 1090m, Stock = 100, ImageUrl = "/images/vanilla-latte.jpg", CategoryId = categories[2].Id },
            new() { Name = "Mocha Latte", Description = "Csokoládés latte", Price = 1190m, Stock = 100, ImageUrl = "/images/mocha-latte.jpg", CategoryId = categories[2].Id },
            new() { Name = "Mandulás Latte", Description = "Mandula ízesítésû latte", Price = 1090m, Stock = 100, ImageUrl = "/images/almond-latte.jpg", CategoryId = categories[2].Id },
            
            // Specialty kategória
            new() { Name = "Flat White", Description = "Ausztrál specialty kávé", Price = 1090m, Stock = 100, ImageUrl = "/images/flat-white.jpg", CategoryId = categories[3].Id },
            new() { Name = "Macchiato", Description = "Espresso tejhabbal", Price = 790m, Stock = 100, ImageUrl = "/images/macchiato.jpg", CategoryId = categories[3].Id },
            new() { Name = "Affogato", Description = "Espresso vaníliafagylalttal", Price = 1290m, Stock = 50, ImageUrl = "/images/affogato.jpg", CategoryId = categories[3].Id },
            new() { Name = "Irish Coffee", Description = "Kávé whiskyvel és tejszínnel", Price = 1490m, Stock = 50, ImageUrl = "/images/irish-coffee.jpg", CategoryId = categories[3].Id },
            
            // Cold Brew kategória
            new() { Name = "Cold Brew", Description = "Hideg fõzésû kávé", Price = 1090m, Stock = 80, ImageUrl = "/images/cold-brew.jpg", CategoryId = categories[4].Id },
            new() { Name = "Iced Latte", Description = "Jeges latte", Price = 1190m, Stock = 80, ImageUrl = "/images/iced-latte.jpg", CategoryId = categories[4].Id },
            new() { Name = "Frappuccino", Description = "Jeges turmix kávé", Price = 1390m, Stock = 80, ImageUrl = "/images/frappuccino.jpg", CategoryId = categories[4].Id },
            
            // Sütemények kategória
            new() { Name = "Croissant", Description = "Vajas croissant", Price = 690m, Stock = 40, ImageUrl = "/images/croissant.jpg", CategoryId = categories[5].Id },
            new() { Name = "Csokis Muffin", Description = "Csokoládés muffin", Price = 790m, Stock = 30, ImageUrl = "/images/choco-muffin.jpg", CategoryId = categories[5].Id },
            new() { Name = "Áfonyás Muffin", Description = "Áfonyás muffin", Price = 790m, Stock = 30, ImageUrl = "/images/blueberry-muffin.jpg", CategoryId = categories[5].Id },
            new() { Name = "Brownie", Description = "Csokoládés brownie", Price = 890m, Stock = 25, ImageUrl = "/images/brownie.jpg", CategoryId = categories[5].Id },
            new() { Name = "Sajttorta", Description = "New York-i sajttorta", Price = 1290m, Stock = 20, ImageUrl = "/images/cheesecake.jpg", CategoryId = categories[5].Id },
            
            // Szendvicsek kategória
            new() { Name = "Club Sandwich", Description = "Csirke, bacon, saláta, paradicsom", Price = 1590m, Stock = 30, ImageUrl = "/images/club-sandwich.jpg", CategoryId = categories[6].Id },
            new() { Name = "Tonhalas Szendvics", Description = "Tonhal, saláta, hagyma", Price = 1390m, Stock = 30, ImageUrl = "/images/tuna-sandwich.jpg", CategoryId = categories[6].Id },
            new() { Name = "Vegán Wrap", Description = "Zöldségekkel töltött tortilla", Price = 1290m, Stock = 25, ImageUrl = "/images/vegan-wrap.jpg", CategoryId = categories[6].Id },
            new() { Name = "Mozzarella Panini", Description = "Mozzarella, paradicsom, bazsalikom", Price = 1490m, Stock = 25, ImageUrl = "/images/panini.jpg", CategoryId = categories[6].Id }
        };

                context.Products.AddRange(products);
                context.SaveChanges();

                var users = new List<Brewed.DataContext.Entities.User>
        {
            new() {
                Name = "Nagy János",
                Email = "nagy.janos@example.com",
                PasswordHash = "hashedpassword123" // Ez csak placeholder, valódi hashelt jelszót kell ide írni
            },
            new() {
                Name = "Kovács Anna",
                Email = "kovacs.anna@example.com",
                PasswordHash = "hashedpassword123"
            },
            new() {
                Name = "Szabó Péter",
                Email = "szabo.peter@example.com",
                PasswordHash = "hashedpassword123"
            },
            new() {
                Name = "Tóth Eszter",
                Email = "toth.eszter@example.com",
                PasswordHash = "hashedpassword123"
            },
            new() {
                Name = "Molnár Gábor",
                Email = "molnar.gabor@example.com",
                PasswordHash = "hashedpassword123"
            }
        };

                context.Users.AddRange(users);
                context.SaveChanges();

                var random = new Random();
                var orders = new List<Brewed.DataContext.Entities.Order>();
                var orderItems = new List<Brewed.DataContext.Entities.OrderItem>();

                foreach (var user in users)
                {
                    var orderCount = random.Next(1, 5);

                    for (int i = 0; i < orderCount; i++)
                    {
                        var order = new Brewed.DataContext.Entities.Order
                        {
                            UserId = user.Id,
                            CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 60)),
                            TotalPrice = 0
                        };

                        orders.Add(order);
                    }
                }

                context.Orders.AddRange(orders);
                context.SaveChanges();

                foreach (var order in orders)
                {
                    var itemCount = random.Next(1, 6);
                    var selectedProducts = products.OrderBy(x => random.Next()).Take(itemCount).ToList();

                    decimal totalPrice = 0;

                    foreach (var product in selectedProducts)
                    {
                        var quantity = random.Next(1, 4);

                        var orderItem = new Brewed.DataContext.Entities.OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = product.Id,
                            Quantity = quantity,
                            UnitPrice = product.Price
                        };

                        orderItems.Add(orderItem);
                        totalPrice += product.Price * quantity;
                    }

                    order.TotalPrice = totalPrice;
                }

                context.OrderItems.AddRange(orderItems);
                context.Orders.UpdateRange(orders);
                context.SaveChanges();
            }
        }
    }
}
