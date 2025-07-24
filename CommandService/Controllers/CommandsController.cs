using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandService.Controllers
{
    // api/c/platforms/12/commands/12
    [Route("api/c/platforms/{platformId}/[controller]")]
    [ApiController]
    public class CommandsController(ICommandRepo commandRepo, IMapper mapper) : ControllerBase
    {
        const string _getCommandForPlatform = "GetCommandForPlatform";

        [HttpGet]
        public async Task<ActionResult<List<CommandReadDto>>> GetCommandsForPlatform(int platformId)
        {
            Console.WriteLine($"Hit GetCommandsForPlatform: {platformId}");
            if (!await commandRepo.PlatformExists(platformId))
            {
                return NotFound();
            }

            var commands = commandRepo.GetCommandsForPlatform(platformId);

            return Ok(mapper.Map<List<CommandReadDto>>(commands));
        }

        [HttpGet("{commandId}", Name = _getCommandForPlatform)]
        public async Task<ActionResult<List<CommandReadDto>>> GetCommandForPlatform(
            int platformId,
            int commandId
        )
        {
            Console.WriteLine($"Hit GetCommandForPlatform: {platformId}, Command: {commandId}");
            if (!await commandRepo.PlatformExists(platformId))
            {
                return NotFound();
            }
            var command = await commandRepo.GetCommand(platformId, commandId);
            if (command == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<CommandReadDto>(command));
        }

        [HttpPost]
        public async Task<ActionResult<CommandReadDto>> CreateCommandForPlatform(
            int platformId,
            CommandCreateDto commandCreateDto
        )
        {
            Console.WriteLine($"Hit CreateCommandForPlatform: {platformId}");
            if (!await commandRepo.PlatformExists(platformId))
            {
                return NotFound();
            }

            try
            {
                var command = mapper.Map<Command>(commandCreateDto);
                await commandRepo.CreateCommand(platformId, command);
                commandRepo.SaveChanges();
                var commandReadDto = mapper.Map<CommandReadDto>(command);
                return CreatedAtRoute(
                    _getCommandForPlatform, // or nameof(CreateCommandForPlatform)
                    new { platformId, commandId = command.Id },
                    commandReadDto
                );
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error from CreateCommandForPlatform: {e.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
