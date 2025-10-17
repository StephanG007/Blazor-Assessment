namespace API.Controllers.Users.DTOs;

public class UserListDto
{
    public required string Id { get; set; }

    public required string DisplayName { get; set; }

    public required string Region { get; set; }

    public required string Country { get; set; }

    public required int Gender { get; set; }

    public string? ImageUrl { get; set; }
}