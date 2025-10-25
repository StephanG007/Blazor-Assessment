using API.Data;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Contracts.Users;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExpress.Blazor;
using DevExtreme.AspNet.Mvc;
using static DevExtreme.AspNet.Data.DataSourceLoader;

namespace API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<LoadResult>> GetUsers([FromQuery] DataSourceLoadOptions loadOptions, CancellationToken ct = default)
    {
        loadOptions.PrimaryKey ??= new[]
        {
            nameof(UserListResponse.Id)
        };
        loadOptions.PaginateViaPrimaryKey ??= true;
        
        var usersQuery = db.Users
            .AsNoTracking()
            .Select(user => new UserListResponse
            {
                Id = user.Id,
                DisplayName = user.Name + " " + user.Surname,
                Region = user.Region,
                Country = user.Country,
                Gender = user.Gender,
                ImageUrl = user.ImageUrl
            });

        var loadResult = await LoadAsync(usersQuery, loadOptions, ct);

        return Ok(loadResult);
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