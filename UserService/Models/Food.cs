using System;
using System.Collections.Generic;

namespace UserService.Models
{
    public partial class Food
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Name { get; set; } = null!;
        public double Price { get; set; }
        public double? Quantity { get; set; }

        public virtual Order Order { get; set; } = null!;
    }
}
