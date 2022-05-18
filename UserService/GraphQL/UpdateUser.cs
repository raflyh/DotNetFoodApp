namespace UserService.GraphQL
{
    public record UpdateUser
    (
        int? Id,
        DateTime? Updated,
        string? Status
    );
}
