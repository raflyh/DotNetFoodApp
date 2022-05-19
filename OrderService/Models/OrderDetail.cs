using System;
using System.Collections.Generic;

namespace OrderService.Models
{
    public partial class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int FoodId { get; set; }
        public double Quantity { get; set; }
        public double? Price { get; set; }
        public DateTime? Created { get; set; }

        public virtual Food Food { get; set; } = null!;
        public virtual Order Order { get; set; } = null!;
    }
}
