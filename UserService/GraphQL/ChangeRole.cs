namespace UserService.GraphQL
{
    public record ChangeRole
    (
        int? Id,
        int? UserId,
        int RoleId
    );
}
