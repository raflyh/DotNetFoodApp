using OrderService.Models;

namespace OrderService.GraphQL
{
    public record BuyFood
    (
        double? DestinationLatitude,
        double? DestinationLongitude,
        List<OrderDetail> OrderDetails
    );
}
