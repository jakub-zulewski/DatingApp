using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseAPIController
{
    private readonly DataContext _dataContext;
    private readonly ITokenService _tokenService;

    public AccountController(DataContext dataContext, ITokenService tokenService)
    {
        _tokenService = tokenService;
        _dataContext = dataContext;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
    {
        if (await UserExists(registerDTO.Username))
            return BadRequest("Username is taken.");

        using var hmac = new HMACSHA512();
        var user = new AppUser
        {
            UserName = registerDTO.Username.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
            PasswordSalt = hmac.Key
        };

        await _dataContext.Users.AddAsync(user);
        await _dataContext.SaveChangesAsync();

        return new UserDTO
        {
            Username = user.UserName,
            Token = _tokenService.CreateToken(user)
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
    {
        var user = await _dataContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserName == loginDTO.Username);

        if (user is null)
            return Unauthorized();

        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i])
                return Unauthorized();
        }

        return new UserDTO
        {
            Username = user.UserName,
            Token = _tokenService.CreateToken(user)
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await _dataContext.Users
            .AsNoTracking()
            .AnyAsync(x => x.UserName == username.ToLower());
    }
}
