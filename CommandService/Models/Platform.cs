using System.ComponentModel.DataAnnotations;

namespace CommandService.Models;

public class Platform
{
    [Key]
    [Required] // ASP.NET Core runtime validation
    public int Id { get; set; }

    [Required]
    public int ExternalId { get; set; }

    [Required]
    public required string Name { get; set; }
    public ICollection<Command> Commands { get; set; } = [];
}
