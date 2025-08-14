using System.ComponentModel.DataAnnotations;

namespace AmazonTrends.WebApp.DTOs;

public class CreateUserDto
{
    [Required]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    public List<string> Roles { get; set; }
}
