using System;
using System.Collections.Generic;

namespace UserService.Models
{
    public partial class Order
    {
        public int Id { get; set; }
        public int FoodId { get; set; }
        public int UserId { get; set; }
        public decimal BuyerLatitude { get; set; }
        public decimal BuyerLongitude { get; set; }
        public decimal? CourierLatitude { get; set; }
        public decimal? CourierLongitude { get; set; }
        public decimal DestinationLatitude { get; set; }
        public decimal DestinationLongitude { get; set; }
        public string Status { get; set; } = null!;
        public double TotalPrice { get; set; }
        public DateTime Created { get; set; }

        public virtual Food Food { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
