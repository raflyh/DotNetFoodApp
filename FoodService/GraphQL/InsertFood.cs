namespace FoodService.GraphQL
{
    public record InsertFood
    (
        int? Id,
        string Name,
        double Price
    );
}
