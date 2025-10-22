namespace Contracts.Account;

public class LoginResponse
{
    public required string Id { get; set; }

    public required string Email { get; set; }

    public required string DisplayName { get; set; }

    public string? ImageUrl { get; set; }

    public required string Token { get; set; }
}
