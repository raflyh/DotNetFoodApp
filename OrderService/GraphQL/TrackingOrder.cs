namespace OrderService.GraphQL
{
    public record TrackingOrder
    (
        double? CourierLatitude,
        double? CourierLongitude
    );
}
