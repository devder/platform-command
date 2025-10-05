using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers;

[Route("api/[controller]")] // this will take the name of the class except the xxx"Controller" suffix
[ApiController]
public class PlatformsController(
    IPlatformRepo repository,
    IMapper mapper,
    ICommandDataClient commandDataClient,
    IMessageBusClient messageBusClient
) : ControllerBase
{
    const string _getPlatformByIdName = "GetPlatformById";

    [HttpGet]
    public async Task<ActionResult<List<PlatformReadDto>>> GetPlatforms()
    {
        Console.WriteLine("--> Getting Platforms");

        var platformItems = await repository.GetAllPlatforms();

        return Ok(mapper.Map<List<PlatformReadDto>>(platformItems));
    }

    [HttpGet("{id}", Name = _getPlatformByIdName)]
    public async Task<ActionResult<PlatformReadDto>> GetPlatformById(int id)
    {
        Console.WriteLine("--> Getting Platform By Id for {0}", id);

        var platformItem = await repository.GetPlatformById(id);

        if (platformItem is null)
        {
            return NotFound();
        }

        return Ok(mapper.Map<PlatformReadDto>(platformItem));
    }

    [HttpPost]
    public async Task<ActionResult<PlatformReadDto>> CreatePlatform(
        PlatformCreateDto platformCreateDto
    )
    {
        Console.WriteLine("--> Creating Platform {0}", platformCreateDto);

        var platformModel = mapper.Map<Platform>(platformCreateDto);
        await repository.CreatePlatform(platformModel);
        repository.SaveChanges();

        var platformReadDto = mapper.Map<PlatformReadDto>(platformModel);
        try
        {
            await commandDataClient.SendPlatformToCommand(platformReadDto);
            Console.WriteLine("Sent synchronously CommandService");
        }
        catch (Exception e)
        {
            Console.WriteLine($"--> Could not send synchronously: {e.Message}");
        }

        try
        {
            var platformPublishedDto = mapper.Map<PlatformPublishedDto>(platformReadDto);
            platformPublishedDto.Event = "Platform_Published";
            messageBusClient.PublishNewPlatform(platformPublishedDto);
            Console.WriteLine("Sent asynchronously to message bus");
        }
        catch (Exception e)
        {
            Console.WriteLine($"--> Could not send asynchronously: {e.Message}");
        }
        return CreatedAtRoute(
            nameof(GetPlatformById), // or _getPlatformByIdName
            new { Id = platformReadDto.Id },
            platformReadDto
        );
    }
}
