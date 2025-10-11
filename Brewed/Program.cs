
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
            new() { Name = "Espresso", Description = "Er�s, koncentr�lt k�v�k espresso alapon" },
            new() { Name = "Cappuccino", Description = "Tejhabos k�v�k�l�nlegess�gek" },
            new() { Name = "Latte", Description = "Tejes k�v�k l�gy �zvil�ggal" },
            new() { Name = "Specialty", Description = "K�l�nleges k�v�kre�ci�k" },
            new() { Name = "Cold Brew", Description = "Hideg f�z�s� k�v�k" },
            new() { Name = "S�tem�nyek", Description = "Frissen s�lt p�ks�tik �s desszertek" },
            new() { Name = "Szendvicsek", Description = "Friss szendvicsek �s snackek" }
        };

                context.Categories.AddRange(categories);
                context.SaveChanges();

                var products = new List<Brewed.DataContext.Entities.Product>
        {
            // Espresso kateg�ria
            new() { Name = "Espresso", Description = "Klasszikus olasz espresso", Price = 590m, Stock = 100, ImageUrl = "/images/espresso.jpg", CategoryId = categories[0].Id },
            new() { Name = "Dupla Espresso", Description = "Dupla adag espresso", Price = 890m, Stock = 100, ImageUrl = "/images/double-espresso.jpg", CategoryId = categories[0].Id },
            new() { Name = "Americano", Description = "Espresso forr� v�zzel", Price = 690m, Stock = 100, ImageUrl = "/images/americano.jpg", CategoryId = categories[0].Id },
            
            // Cappuccino kateg�ria
            new() { Name = "Cappuccino", Description = "Espresso tejes habbal", Price = 890m, Stock = 100, ImageUrl = "/images/cappuccino.jpg", CategoryId = categories[1].Id },
            new() { Name = "Van�lia Cappuccino", Description = "Van�li�s cappuccino", Price = 990m, Stock = 100, ImageUrl = "/images/vanilla-cappuccino.jpg", CategoryId = categories[1].Id },
            new() { Name = "Karamell Cappuccino", Description = "Karamell�s cappuccino", Price = 990m, Stock = 100, ImageUrl = "/images/caramel-cappuccino.jpg", CategoryId = categories[1].Id },
            
            // Latte kateg�ria
            new() { Name = "Caff? Latte", Description = "Klasszikus tejesk�v�", Price = 990m, Stock = 100, ImageUrl = "/images/latte.jpg", CategoryId = categories[2].Id },
            new() { Name = "Van�lia Latte", Description = "Van�li�s latte", Price = 1090m, Stock = 100, ImageUrl = "/images/vanilla-latte.jpg", CategoryId = categories[2].Id },
            new() { Name = "Mocha Latte", Description = "Csokol�d�s latte", Price = 1190m, Stock = 100, ImageUrl = "/images/mocha-latte.jpg", CategoryId = categories[2].Id },
            new() { Name = "Mandul�s Latte", Description = "Mandula �zes�t�s� latte", Price = 1090m, Stock = 100, ImageUrl = "/images/almond-latte.jpg", CategoryId = categories[2].Id },
            
            // Specialty kateg�ria
            new() { Name = "Flat White", Description = "Ausztr�l specialty k�v�", Price = 1090m, Stock = 100, ImageUrl = "/images/flat-white.jpg", CategoryId = categories[3].Id },
            new() { Name = "Macchiato", Description = "Espresso tejhabbal", Price = 790m, Stock = 100, ImageUrl = "/images/macchiato.jpg", CategoryId = categories[3].Id },
            new() { Name = "Affogato", Description = "Espresso van�liafagylalttal", Price = 1290m, Stock = 50, ImageUrl = "/images/affogato.jpg", CategoryId = categories[3].Id },
            new() { Name = "Irish Coffee", Description = "K�v� whiskyvel �s tejsz�nnel", Price = 1490m, Stock = 50, ImageUrl = "/images/irish-coffee.jpg", CategoryId = categories[3].Id },
            
            // Cold Brew kateg�ria
            new() { Name = "Cold Brew", Description = "Hideg f�z�s� k�v�", Price = 1090m, Stock = 80, ImageUrl = "/images/cold-brew.jpg", CategoryId = categories[4].Id },
            new() { Name = "Iced Latte", Description = "Jeges latte", Price = 1190m, Stock = 80, ImageUrl = "/images/iced-latte.jpg", CategoryId = categories[4].Id },
            new() { Name = "Frappuccino", Description = "Jeges turmix k�v�", Price = 1390m, Stock = 80, ImageUrl = "/images/frappuccino.jpg", CategoryId = categories[4].Id },
            
            // S�tem�nyek kateg�ria
            new() { Name = "Croissant", Description = "Vajas croissant", Price = 690m, Stock = 40, ImageUrl = "/images/croissant.jpg", CategoryId = categories[5].Id },
            new() { Name = "Csokis Muffin", Description = "Csokol�d�s muffin", Price = 790m, Stock = 30, ImageUrl = "/images/choco-muffin.jpg", CategoryId = categories[5].Id },
            new() { Name = "�fony�s Muffin", Description = "�fony�s muffin", Price = 790m, Stock = 30, ImageUrl = "/images/blueberry-muffin.jpg", CategoryId = categories[5].Id },
            new() { Name = "Brownie", Description = "Csokol�d�s brownie", Price = 890m, Stock = 25, ImageUrl = "/images/brownie.jpg", CategoryId = categories[5].Id },
            new() { Name = "Sajttorta", Description = "New York-i sajttorta", Price = 1290m, Stock = 20, ImageUrl = "/images/cheesecake.jpg", CategoryId = categories[5].Id },
            
            // Szendvicsek kateg�ria
            new() { Name = "Club Sandwich", Description = "Csirke, bacon, sal�ta, paradicsom", Price = 1590m, Stock = 30, ImageUrl = "/images/club-sandwich.jpg", CategoryId = categories[6].Id },
            new() { Name = "Tonhalas Szendvics", Description = "Tonhal, sal�ta, hagyma", Price = 1390m, Stock = 30, ImageUrl = "/images/tuna-sandwich.jpg", CategoryId = categories[6].Id },
            new() { Name = "Veg�n Wrap", Description = "Z�lds�gekkel t�lt�tt tortilla", Price = 1290m, Stock = 25, ImageUrl = "/images/vegan-wrap.jpg", CategoryId = categories[6].Id },
            new() { Name = "Mozzarella Panini", Description = "Mozzarella, paradicsom, bazsalikom", Price = 1490m, Stock = 25, ImageUrl = "/images/panini.jpg", CategoryId = categories[6].Id }
        };

                context.Products.AddRange(products);
                context.SaveChanges();

                var users = new List<Brewed.DataContext.Entities.User>
        {
            new() {
                Name = "Nagy J�nos",
                Email = "nagy.janos@example.com",
                PasswordHash = "hashedpassword123" // Ez csak placeholder, val�di hashelt jelsz�t kell ide �rni
            },
            new() {
                Name = "Kov�cs Anna",
                Email = "kovacs.anna@example.com",
                PasswordHash = "hashedpassword123"
            },
            new() {
                Name = "Szab� P�ter",
                Email = "szabo.peter@example.com",
                PasswordHash = "hashedpassword123"
            },
            new() {
                Name = "T�th Eszter",
                Email = "toth.eszter@example.com",
                PasswordHash = "hashedpassword123"
            },
            new() {
                Name = "Moln�r G�bor",
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
