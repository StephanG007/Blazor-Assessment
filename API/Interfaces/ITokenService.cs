using API.Data.Entities;

namespace API.Interfaces;

public interface ITokenService
{
    Task<string> CreateToken(User user);
}
