using AmazonTrends.Data.Models;
using AmazonTrends.WebApp.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmazonTrends.WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: api/admin/users
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = await _userManager.GetRolesAsync(user)
            });
        }

        return Ok(userDtos);
    }

    // GET: api/admin/users/{id}
    [HttpGet("users/{id}")]
    public async Task<ActionResult<UserDto>> GetUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Roles = await _userManager.GetRolesAsync(user)
        };

        return Ok(userDto);
    }

    // POST: api/admin/users
    [HttpPost("users")]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
    {
        var user = new ApplicationUser
        {
            UserName = createUserDto.UserName,
            Email = createUserDto.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, createUserDto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        if (createUserDto.Roles != null)
        {
            await _userManager.AddToRolesAsync(user, createUserDto.Roles);
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Roles = await _userManager.GetRolesAsync(user)
        };

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDto);
    }

    // PUT: api/admin/users/{id}
    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(string id, UpdateUserDto updateUserDto)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        user.Email = updateUserDto.Email;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        var existingRoles = await _userManager.GetRolesAsync(user);
        var resultRemove = await _userManager.RemoveFromRolesAsync(user, existingRoles);

        if (!resultRemove.Succeeded)
        {
            return BadRequest(resultRemove.Errors);
        }

        if (updateUserDto.Roles != null)
        {
            var resultAdd = await _userManager.AddToRolesAsync(user, updateUserDto.Roles);
            if (!resultAdd.Succeeded)
            {
                return BadRequest(resultAdd.Errors);
            }
        }

        return NoContent();
    }

    // DELETE: api/admin/users/{id}
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return NoContent();
    }

    // GET: api/admin/roles
    [HttpGet("roles")]
    public async Task<ActionResult<IEnumerable<string>>> GetRoles()
    {
        var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        return Ok(roles);
    }
}