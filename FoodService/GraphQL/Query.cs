using FoodService.Models;
using HotChocolate.AspNetCore.Authorization;

namespace FoodService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new [] { "MANAGER", "BUYER"})]
        public IQueryable<Food> GetFoods([Service] DotNetFoodDbContext context)
        {
            return context.Foods.AsQueryable();
        }
    }
}
