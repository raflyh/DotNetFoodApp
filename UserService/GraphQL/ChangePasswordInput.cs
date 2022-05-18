namespace UserService.GraphQL
{
    public record ChangePasswordInput
    (
        string Password,
        DateTime? Updated
    );
}
