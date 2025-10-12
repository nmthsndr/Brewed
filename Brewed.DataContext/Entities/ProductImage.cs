using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brewed.DataContext.Entities
{
    public class ProductImage
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string ImageUrl { get; set; }

        public int DisplayOrder { get; set; }

        public int ProductId { get; set; }

        public virtual Product Product { get; set; }
    }
}
