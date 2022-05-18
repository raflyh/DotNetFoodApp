using System;
using System.Collections.Generic;

namespace UserService.Models
{
    public partial class Order
    {
        public int Id { get; set; }
        public int FoodId { get; set; }
        public int UserId { get; set; }
        public double? BuyerLatitude { get; set; }
        public double? BuyerLongitude { get; set; }
        public double? CourierLatitude { get; set; }
        public double? CourierLongitude { get; set; }
        public double? DestinationLatitude { get; set; }
        public double? DestinationLongitude { get; set; }
        public string Status { get; set; } = null!;
        public double TotalPrice { get; set; }
        public DateTime? Created { get; set; }

        public virtual Food Food { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
