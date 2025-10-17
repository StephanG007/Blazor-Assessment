using API.Controllers.Account.DTOs;
using API.Data.Entities;
using API.Interfaces;
using API.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.Account;

[Route("api/[controller]")]
[ApiController]
public class AccountController(UserManager<User> db, ITokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest dto)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == dto.Email);
        var result = await db.CheckPasswordAsync(user!, dto.Password);

        if (!result) return Unauthorized("Invalid email or password");

        return await user!.ToLoginDto(tokenService);
    }
}