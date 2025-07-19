using System.ComponentModel.DataAnnotations;

namespace PlatformService.Models;

public class Platform
{
    [Key]
    [Required] // ASP.NET Core runtime validation
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Publisher { get; set; }

    [Required]
    public required string Cost { get; set; }
}
