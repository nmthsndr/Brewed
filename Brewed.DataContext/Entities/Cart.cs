using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brewed.DataContext.Entities
{
    public class Cart
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        [StringLength(100)]
        public string SessionId { get; set; } // For guest users

        public virtual User User { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
