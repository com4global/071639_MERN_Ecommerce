using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using ApiOnLamda.Model;
using ApiOnLamda.Data;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly DatabaseHelper _dbHelper;

    public UserController(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] User user)
    {
        var existingUser = await _dbHelper.GetUserByEmail(user.Email);
        if (existingUser != null)
        {
            return BadRequest("User already exists.");
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        var userId = await _dbHelper.AddUser(user);

        return Ok(new { Id = userId, Message = "Signup successful.", Role = user.Role });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest login)
    {
        if (login == null || string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password))
        {
            return BadRequest("Invalid login data.");
        }
        var user = await _dbHelper.GetUserByEmail(login.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
        {
            return Unauthorized("Invalid email or password.");
        }

        return Ok(new { Token = "generated_jwt_token_here", Role = user.Role,user.Id });
    }
}
