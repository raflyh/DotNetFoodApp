using UserService.Models;

namespace UserService.GraphQL
{
    public record RegisterUser
    (
        int? Id,
        string Username,
        string Email,
        string Password,
        float? Latitude,
        float? Longitude,
        DateTime? Created,
        DateTime? Updated,
        string? Status
    );
}
