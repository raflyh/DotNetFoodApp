using System;
using System.Collections.Generic;

namespace OrderService.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Code { get; set; }
        public double? BuyerLatitude { get; set; }
        public double? BuyerLongitude { get; set; }
        public double? CourierLatitude { get; set; }
        public double? CourierLongitude { get; set; }
        public double? DestinationLatitude { get; set; }
        public double? DestinationLongitude { get; set; }
        public string Status { get; set; } = null!;
        public double TotalPrice { get; set; }
        public DateTime? Created { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
