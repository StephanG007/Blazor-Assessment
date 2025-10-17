using API.Controllers.Users.DTOs;
using API.Data;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.Users;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<UserListDto>> GetUsers()
    {
        var users = await db.Users.Select(user => new UserListDto
        {
            Id = user.Id,
            DisplayName = user.Name + " " + user.Surname,
            Region = user.Region,
            Country = user.Country,
            Gender = user.Gender,
            ImageUrl = user.ImageUrl
        }).ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile(string id)
    {
        var user = await db.Users.FindAsync(id);

        if (user == null) return NotFound();

        return user.ToProfileDto();
    }
}