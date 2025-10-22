using API.Data;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Contracts.Users;

namespace API.Controllers.Users;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<IEnumerable<UserListResponse>>> GetUsers()
    {
        var users = await db.Users.Select(user => new UserListResponse
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
    public async Task<ActionResult<UserProfileResponse>> GetUserProfile(string id)
    {
        var user = await db.Users.FindAsync(id);

        if (user == null) return NotFound();

        return user.ToProfileResponse();
    }
}