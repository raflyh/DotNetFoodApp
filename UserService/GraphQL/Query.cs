using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.Models;

namespace UserService.GraphQL
{
    public class Query
    {
        [Authorize]
        public IQueryable<User> GetUsers([Service] DotNetFoodDbContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;

            var userRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role).FirstOrDefault();
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (user != null)
            {
                if (userRole != null)
                {
                    if (userRole.Value == "ADMIN")
                    {
                        return context.Users.Include(p => p.Profiles).AsQueryable();
                    }
                }
                    
                var users = context.Users.Include(p => p.Profiles).Where(o => o.Id == user.Id);

                return users.AsQueryable();
            }
            return new List<User>().AsQueryable();
        }

        [Authorize(Roles = new[] {"MANAGER"})]
        public IQueryable<User> GetCouriers([Service] DotNetFoodDbContext context)
        {
            var courierRole = context.Roles.Where(a => a.Name == "COURIER").FirstOrDefault();
            var couriers = context.Users.Include(p => p.Profiles).Where(a => a.UserRoles.Any(o => o.RoleId == courierRole.Id));
            return couriers.AsQueryable();
        }
        
        [Authorize]
        public IQueryable<Balance> GetBalances(ClaimsPrincipal claimsPrincipal, [Service] DotNetFoodDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var userBalance = context.Balances.Where(u => u.UserId == user.Id);
            return userBalance.AsQueryable();
        }

    }
}
