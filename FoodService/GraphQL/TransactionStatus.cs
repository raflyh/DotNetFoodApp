namespace FoodService.GraphQL
{
    public record TransactionStatus
     (
         bool Success,
         string? Message
     );
}
