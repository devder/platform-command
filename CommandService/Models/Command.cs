using System.ComponentModel.DataAnnotations;

namespace CommandService.Models;

public class Command
{
    [Key]
    [Required] // ASP.NET Core runtime validation
    public int Id { get; set; }

    [Required]
    public required string HowTo { get; set; }

    [Required]
    public required string CommandLine { get; set; }

    [Required]
    public int PlatformId { get; set; }

    public Platform? Platform { get; set; }
}
