using Microsoft.AspNetCore.Identity;

namespace API.Data.Entities;

public class User : IdentityUser
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

  public string? RefreshToken { get; set; }
  public DateTime? RefreshTokenExpiry { get; set; }
}