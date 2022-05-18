using System;
using System.Collections.Generic;

namespace FoodService.Models
{
    public partial class Food
    {
        public Food()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public double Price { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
