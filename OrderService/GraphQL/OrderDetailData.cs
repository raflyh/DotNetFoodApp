namespace OrderService.GraphQL
{
    public record OrderDetailData
    (
        int? Id,
        int? OrderId,
        int FoodId,
        double Quantity,
        DateTime? Created
    );
}
