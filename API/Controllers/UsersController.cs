using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class UsersController : BaseAPIController
{
    private readonly DataContext _dataContext;

    public UsersController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
    {
        return await _dataContext.Users.AsNoTracking().ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppUser>> GetUser(int id)
    {
        return await _dataContext.Users.FindAsync(id);
    }
}
