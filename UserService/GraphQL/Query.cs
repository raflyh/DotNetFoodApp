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

            var managerRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role).FirstOrDefault();
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (user != null)
            {
                if (managerRole != null)
                    if (managerRole.Value == "ADMIN")
                    {
                        return context.Users.Include(p => p.Profiles);
                    }
                var users = context.Users.Include(p => p.Profiles).Where(o => o.Id == user.Id);

                return users.AsQueryable();
            }
            return new List<User>().AsQueryable();
        }
    }
}
