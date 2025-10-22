namespace Contracts.Users;

public class UserProfileResponse
{
    public required string Name { get; set; }

    public required string Surname { get; set; }

    public required string DisplayName { get; set; }

    public required string Phone { get; set; }

    public required string Address { get; set; }

    public required string PostalZip { get; set; }

    public required string Region { get; set; }

    public required string Country { get; set; }

    public required int Gender { get; set; }

    public string? ImageUrl { get; set; }
}
