using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OrderService.Models;
using System.Security.Claims;

namespace OrderService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "MANAGER", "BUYER" })]
        public IQueryable<Order> GetOrders(ClaimsPrincipal claimsPrincipal,[Service] DotNetFoodDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;

            var userRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role).FirstOrDefault();
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (user != null)
            {
                if (userRole != null)
                {
                    if (userRole.Value == "MANAGER")
                    {
                        return context.Orders.Include(p => p.OrderDetails).ThenInclude(o => o.Food).AsQueryable();
                    }
                }
                var orders = context.Orders.Include(p => p.OrderDetails).ThenInclude(o => o.Food).Where(a => a.BuyerId == user.Id);
                return orders.AsQueryable();
            }
            return new List<Order>().AsQueryable();
        }
    }
}
