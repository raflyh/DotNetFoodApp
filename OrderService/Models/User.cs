using System;
using System.Collections.Generic;

namespace OrderService.Models
{
    public partial class User
    {
        public User()
        {
            Balances = new HashSet<Balance>();
            Orders = new HashSet<Order>();
            UserRoles = new HashSet<UserRole>();
        }

        public int Id { get; set; }
        public string Fullname { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual ICollection<Balance> Balances { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
