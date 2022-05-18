namespace UserService.GraphQL
{
    public record EditProfile
    (
        int? Id,
        string Name,
        string Address,
        string Phone
    );
}
