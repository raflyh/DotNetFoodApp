namespace OrderService.GraphQL
{
    public record UpdateOrder
    (
        int? Id,
        int? CourierId,
        string Status
    );
}
