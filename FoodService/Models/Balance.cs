using System;
using System.Collections.Generic;

namespace FoodService.Models
{
    public partial class Balance
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public double BalanceTotal { get; set; }
        public double BalanceMutation { get; set; }
        public DateTime? Date { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
