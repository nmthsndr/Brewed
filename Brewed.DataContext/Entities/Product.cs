namespace Brewed.DataContext.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string RoastLevel { get; set; } = "N/A"; // Light, Medium, Dark
        public string Origin { get; set; } = "N/A";
        public bool IsCaffeineFree { get; set; } = false;
        public bool IsOrganic { get; set; } = false;
        public string ImageUrl { get; set; } = null!;

        public virtual Category Category { get; set; }
        public virtual ICollection<ProductImage> ProductImages { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
