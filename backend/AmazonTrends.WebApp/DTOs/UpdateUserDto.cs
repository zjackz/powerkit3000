using System.ComponentModel.DataAnnotations;

namespace AmazonTrends.WebApp.DTOs;

public class UpdateUserDto
{
    [EmailAddress]
    public string Email { get; set; }

    public List<string> Roles { get; set; }
}
