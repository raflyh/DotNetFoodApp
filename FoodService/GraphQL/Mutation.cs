using FoodService.Models;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;

namespace FoodService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<TransactionStatus> AddFoodAsync(
            InsertFood input,
            [Service] DotNetFoodDbContext context)
        {
            var newFood = new Food
            {
                Name = input.Name,
                Price = input.Price
            };
            context.Foods.Add(newFood);
            await context.SaveChangesAsync();

            return await Task.FromResult(new TransactionStatus
            (
                true, "Food Has Been Saved"
            ));
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<TransactionStatus> UpdateFoodAsync(
            int id,
            InsertFood input,
            [Service] DotNetFoodDbContext context)
        {
            var food = context.Foods.Where(f=>f.Id==id).FirstOrDefault();
            if (food == null)
            {
                return await Task.FromResult(new TransactionStatus
            (
                false, "Food not Found!"
            ));
            }

            food.Name = input.Name;
            food.Price = input.Price;

            context.Foods.Update(food);
            await context.SaveChangesAsync();

            return await Task.FromResult(new TransactionStatus
            (
                true, "Food Has Been Saved"
            ));
        }
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<TransactionStatus> DeleteFoodAsync(
            int id,
            [Service] DotNetFoodDbContext context)
        {
            var food = context.Foods.Where(f => f.Id == id).FirstOrDefault();
            if (food == null)
            {
                return await Task.FromResult(new TransactionStatus
            (
                false, "Food not Found!"
            ));
            }
            context.Foods.Remove(food);
            await context.SaveChangesAsync();

            return await Task.FromResult(new TransactionStatus
            (
                true, "Food Has Been Deleted"
            ));
        }
    }
}
