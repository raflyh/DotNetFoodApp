using System;
using System.Collections.Generic;

namespace FoodService.Models
{
    public partial class User
    {
        public User()
        {
            Balances = new HashSet<Balance>();
            OrderBuyers = new HashSet<Order>();
            OrderCouriers = new HashSet<Order>();
            Profiles = new HashSet<Profile>();
            UserRoles = new HashSet<UserRole>();
        }

        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Updated { get; set; }
        public string Status { get; set; } = null!;

        public virtual ICollection<Balance> Balances { get; set; }
        public virtual ICollection<Order> OrderBuyers { get; set; }
        public virtual ICollection<Order> OrderCouriers { get; set; }
        public virtual ICollection<Profile> Profiles { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
