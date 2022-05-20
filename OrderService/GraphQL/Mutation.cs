using GeoCoordinatePortable;
using HotChocolate.AspNetCore.Authorization;
using OrderService.Models;
using System.Security.Claims;

namespace OrderService.GraphQL
{
    public class Mutation
    {
        //Buyer Privilege
        [Authorize(Roles = new[] { "BUYER" })]
        public async Task<TransactionStatus> AddOrderAsync(
            BuyFood input,
            ClaimsPrincipal claimsPrincipal,
            [Service] DotNetFoodDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var buyer = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var buyerBalance = context.Balances.Where(o => o.UserId == buyer.Id).OrderBy(p => p.Id).LastOrDefault();
            var userCoord = new GeoCoordinate(buyer.Latitude, buyer.Longitude);
            var destiCoord = new GeoCoordinate((double)input.DestinationLatitude, (double)input.DestinationLongitude);
            var distance = userCoord.GetDistanceTo(destiCoord) / 1000; //distance in km
            double pricePerKm = 3000; //harga per km
            var distancePrice = distance * pricePerKm;

            var order = new Order
            {
                Code = Guid.NewGuid().ToString(), // generate random chars using GUID
                BuyerId = buyer.Id,
                CourierId = null,// added when order is accepted
                BuyerLatitude = buyer.Latitude,
                BuyerLongitude = buyer.Longitude,
                DestinationLatitude = input.DestinationLatitude,
                DestinationLongitude = input.DestinationLongitude,
                CourierLatitude = null,// added when order is accepted
                CourierLongitude = null,// added when order is accepted
                Created = DateTime.Now,
                TotalPrice = 0,
                Status = "PENDING"
            };
            
            double foodPrice = 0;
            foreach (var item in input.OrderDetails)
            {
                var detail = new OrderDetail
                {
                    OrderId = order.Id,
                    FoodId = item.FoodId,
                    Quantity = item.Quantity
                };
                var food = context.Foods.Where(o => o.Id == detail.FoodId).FirstOrDefault();
                order.OrderDetails.Add(detail);
                foodPrice += food.Price * detail.Quantity;
            }
            order.TotalPrice=distancePrice+foodPrice;
            context.Orders.Add(order);
            if (buyerBalance.BalanceTotal >= order.TotalPrice)
            {
                if(buyer.Status == "Free")
                {
                    var newBalance = new Balance
                    {
                        UserId = buyer.Id,
                        BalanceTotal = buyerBalance.BalanceTotal - order.TotalPrice,
                        BalanceMutation = -order.TotalPrice,
                        Date = DateTime.Now
                    };
                    context.Balances.Add(newBalance);

                    buyer.Status = "PENDING";
                    context.Users.Update(buyer);

                    await context.SaveChangesAsync();

                    return await Task.FromResult(new TransactionStatus
                    (
                        true, $"Order Success! Order Fee:{order.TotalPrice.ToString()}. Waiting for Courier..."
                    ));
                }
                return await Task.FromResult(new TransactionStatus
                    (
                        false, "Order Failed! Please Finish Your Last Order!"
                    ));
            }
            else
            {
                return await Task.FromResult(new TransactionStatus
                (
                    false, "Order Failed, Please Check Your Balance!"
                ));
            }
        }
        //Manager Privilege
        [Authorize(Roles = new[] {"MANAGER"})]
        public async Task<TransactionStatus> UpdateOrderAsync(
            int id,
            UpdateOrder input,
            [Service] DotNetFoodDbContext context)
        {
            var order = context.Orders.Where(o => o.Id == id).FirstOrDefault();
            if (order == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Order not Found!"));
            }
            var courier = context.Users.Where(o => o.Id == input.CourierId).FirstOrDefault();
            order.CourierId = input.CourierId;
            order.CourierLatitude = courier.Latitude;
            order.CourierLongitude = courier.Longitude;
            order.Status = "ACCEPTED";

            context.Orders.Update(order);

            await context.SaveChangesAsync();

            return await Task.FromResult(new TransactionStatus
            (
                true, "Order Updated"
            ));
        }

        [Authorize(Roles = new[] {"MANAGER"})]
        public async Task<TransactionStatus> DeleteOrderAsync(
            int id,
            [Service] DotNetFoodDbContext context)
        {
            var order = context.Orders.Where(o => o.Id == id).FirstOrDefault();
            if (order == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Order not Found!"));
            }
            if (order.Status == "CANCELLED")
            {
                context.Orders.Remove(order);
                await context.SaveChangesAsync();
                return await Task.FromResult(new TransactionStatus
            (
                true, "Order Deleted"
            ));
            }
            order.Status = "CANCELLED";

            context.Orders.Update(order);

            await context.SaveChangesAsync();

            return await Task.FromResult(new TransactionStatus
            (
                true, "Order Cancelled"
            ));
        }
        //Courier Privilege
        [Authorize(Roles = new [] {"COURIER"})]
        public async Task<TransactionStatus> AcceptOrderAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] DotNetFoodDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var courier = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if(courier == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Courier not Found!"));
            }
            var order = context.Orders.Where(o => o.Status == "PENDING").OrderBy(o => o.Created).FirstOrDefault();
            var buyer = context.Users.Where(o => o.Id == order.BuyerId).FirstOrDefault();

            var courierCoord = new GeoCoordinate(courier.Latitude, courier.Longitude);
            var destiCoord = new GeoCoordinate((double)order.DestinationLatitude, (double)order.DestinationLongitude);
            var distance = courierCoord.GetDistanceTo(destiCoord)/1000;
            if(courier.Status == "Free")
            {
                if (distance > 25)
                {
                    return await Task.FromResult(new TransactionStatus(false, "Courier too Far!"));
                }
                //update user status
                courier.Status = "BOOKED";
                buyer.Status = "BOOKED";
                context.Users.Update(courier);
                context.Users.Update(buyer);
                //update order
                order.CourierId = courier.Id;
                order.CourierLatitude = courier.Latitude;
                order.CourierLongitude = courier.Longitude;
                order.Status = "ACCEPTED";

                context.Orders.Update(order);
                await context.SaveChangesAsync();

                return await Task.FromResult(new TransactionStatus
                (
                    true, "Order Accepted!"
                ));
            }
            return await Task.FromResult(new TransactionStatus
            (
                false, "Order Failed! Please Finish Your Last Order!"
            ));
        }

        [Authorize(Roles = new[] {"COURIER"})]
        public async Task<TransactionStatus> TrackingOrderAsync(
            int id,
            TrackingOrder input,
            ClaimsPrincipal claimsPrincipal,
            [Service] DotNetFoodDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var courier = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (courier == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Courier not Found!"));
            }

            var order = context.Orders.Where(o => (o.Status == "ACCEPTED" || o.Status.Contains("SENDING")) && o.CourierId == courier.Id && o.Id == id).FirstOrDefault();
            var buyer = context.Users.Where(o => o.Id == order.BuyerId).FirstOrDefault();

            if (order == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Order not Found!"));
            }
            //update order status

            order.CourierLatitude = input.CourierLatitude;
            order.CourierLongitude = input.CourierLongitude;
            var buyerCoord = new GeoCoordinate((double)order.BuyerLatitude, (double)order.BuyerLongitude);
            var courierCoord = new GeoCoordinate((double)order.CourierLatitude, (double)order.CourierLongitude);
            var distance = buyerCoord.GetDistanceTo(courierCoord)/1000;
            order.Status = $"SENDING, COURIER DISTANCE {distance.ToString("N3")} Km";
            context.Orders.Update(order);

            await context.SaveChangesAsync();

            return await Task.FromResult(new TransactionStatus
            (
                true, "Order Tracked!"
            ));
        }

        [Authorize(Roles = new[] { "COURIER" })]
        public async Task<TransactionStatus> FinishOrderAsync(
            int id,
            ClaimsPrincipal claimsPrincipal,
            [Service] DotNetFoodDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var courier = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (courier == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Courier not Found!"));
            }

            var order = context.Orders.Where(o => o.Status.Contains("SENDING") && o.CourierId==courier.Id && o.Id == id).FirstOrDefault();
            var buyer = context.Users.Where(o => o.Id == order.BuyerId).FirstOrDefault();

            if (order==null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Order not Found!"));
            }
            //update user status
            courier.Status = "Free";
            buyer.Status = "Free";
            context.Users.Update(courier);
            context.Users.Update(buyer);
            //update order status
            order.Status = "FINISHED";
            context.Orders.Update(order);

            var courierBalance = context.Balances.Where(o => o.UserId == courier.Id).OrderByDescending(o => o.Date).FirstOrDefault();
            if (courierBalance == null)
            {
                var newBalance = new Balance
                {
                    UserId = courier.Id,
                    BalanceTotal = order.TotalPrice,
                    BalanceMutation = order.TotalPrice,
                    Date = DateTime.Now
                };
                context.Balances.Add(newBalance);
            }
            else
            {
                var addBalance = new Balance
                {
                    UserId = courier.Id,
                    BalanceTotal = courierBalance.BalanceTotal + order.TotalPrice,
                    BalanceMutation = order.TotalPrice,
                    Date = DateTime.Now
                };
                context.Balances.Add(addBalance);
            }
            await context.SaveChangesAsync();
            return await Task.FromResult(new TransactionStatus
            (
                true, "Order Finished!"
            ));
        }

        [Authorize(Roles = new[] { "BUYER", "COURIER" })]
        public async Task<TransactionStatus> CancelOrderAsync(
            int id,
            ClaimsPrincipal claimsPrincipal,
            [Service] DotNetFoodDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var userRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role).FirstOrDefault();
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            
            var order = context.Orders.Where(o => o.Id == id).FirstOrDefault();
            var buyer = context.Users.Where(o => o.Id == order.BuyerId).FirstOrDefault();
            var courier = context.Users.Where(o => o.Id == order.CourierId).FirstOrDefault();
            var buyerBalance = context.Balances.Where(o => o.UserId == buyer.Id).OrderBy(o => o.Id).LastOrDefault();

            if (order == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Order not Found!"));
            }
            if (user != null)
            {
                if (userRole != null)
                {
                    if (userRole.Value == "BUYER")
                    {
                        order.Status = "CANCELLED BY BUYER";
                        context.Orders.Update(order);
                    }
                    if (userRole.Value == "COURIER")
                    {
                        order.Status = "CANCELLED BY COURIER. LOOKING FOR ANOTHER COURIER....";
                        context.Orders.Update(order);
                    }
                    buyer.Status = "Free";
                    courier.Status = "Free";
                    context.Users.Update(buyer);
                    context.Users.Update(courier);

                    var newBalance = new Balance
                    {
                        UserId = buyer.Id,
                        BalanceTotal = buyerBalance.BalanceTotal + order.TotalPrice,
                        BalanceMutation = order.TotalPrice,
                        Date = DateTime.Now
                    };
                    context.Balances.Add(newBalance);  
                    await context.SaveChangesAsync();
                    return await Task.FromResult(new TransactionStatus
                    (
                        true, "Order CancelLed! Balance Refunded!"
                    ));
                }
            }
            return await Task.FromResult(new TransactionStatus
            (
                false, "Failed to Cancel Order!"
            ));
        }
    }
}
