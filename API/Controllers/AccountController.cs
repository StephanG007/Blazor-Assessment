using API.Extensions;
using API.Data.Entities;
using API.Interfaces;
using Contracts.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController(UserManager<User> userManager, ITokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest dto)
    {
        var user = await userManager.Users.SingleOrDefaultAsync(x => x.Email == dto.Email);

        if (user is null || !await userManager.CheckPasswordAsync(user, dto.Password))
        {
            return Unauthorized("Invalid email or password");
        }

        var response = await user.ToLoginResponse(tokenService);

        return Ok(response);
    }
}
