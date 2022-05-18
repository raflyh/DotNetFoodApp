namespace UserService.GraphQL
{
    public record TransactionStatus
    (
        bool Success,
        string? Message
    );
}
