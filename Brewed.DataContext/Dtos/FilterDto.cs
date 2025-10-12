using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brewed.DataContext.Dtos
{
    public class ProductFilterDto
    {
        public int? CategoryId { get; set; }
        public string Search { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string RoastLevel { get; set; }
        public string Origin { get; set; }
        public bool? IsCaffeineFree { get; set; }
        public bool? IsOrganic { get; set; }
        public string SortBy { get; set; } = "name"; // name, price-asc, price-desc, rating
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}
