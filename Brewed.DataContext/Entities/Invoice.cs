using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brewed.DataContext.Entities
{
    public class Invoice
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        [StringLength(200)]
        public string PdfUrl { get; set; }

        public int OrderId { get; set; }

        public virtual Order Order { get; set; }
    }
}
