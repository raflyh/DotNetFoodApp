namespace OrderService.GraphQL
{
    public record TransactionStatus
    (
        bool Success,
        string? Message
    );
}
