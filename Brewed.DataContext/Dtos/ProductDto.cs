using System.ComponentModel.DataAnnotations;

namespace Brewed.Dtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string RoastLevel { get; set; }
        public string Origin { get; set; }
        public bool IsCaffeineFree { get; set; }
        public bool IsOrganic { get; set; }
        public string ImageUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<ProductImageDto> ProductImages { get; set; }
    }

    public class ProductCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Required]
        [StringLength(50)]
        public string RoastLevel { get; set; }

        [Required]
        [StringLength(100)]
        public string Origin { get; set; }

        public bool IsCaffeineFree { get; set; }

        public bool IsOrganic { get; set; }

        [Required]
        [StringLength(200)]
        public string ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }

    public class ProductUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Required]
        [StringLength(50)]
        public string RoastLevel { get; set; }

        [Required]
        [StringLength(100)]
        public string Origin { get; set; }

        public bool IsCaffeineFree { get; set; }

        public bool IsOrganic { get; set; }

        [Required]
        [StringLength(200)]
        public string ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }

    public class ProductImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
    }
}
