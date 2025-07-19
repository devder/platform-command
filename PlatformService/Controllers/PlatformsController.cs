using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;

namespace PlatformService.Controllers;

[Route("api/[controller]")] // this will take the name of the class except the xxx"Controller" suffix
[ApiController]
public class PlatformsController(IPlatformRepo repository, IMapper mapper) : ControllerBase
{
    private readonly IPlatformRepo _repository = repository;
    private readonly IMapper _mapper = mapper;
    const string _getPlatformByIdName = "GetPlatformById";

    [HttpGet]
    public async Task<ActionResult<List<PlatformReadDto>>> GetPlatforms()
    {
        Console.WriteLine("--> Getting Platforms");

        var platformItems = await _repository.GetAllPlatforms();

        return Ok(_mapper.Map<List<PlatformReadDto>>(platformItems));
    }

    [HttpGet("{id}", Name = _getPlatformByIdName)]
    public async Task<ActionResult<PlatformReadDto>> GetPlatformById(int id)
    {
        Console.WriteLine("--> Getting Platform By Id for {0}", id);

        var platformItem = await _repository.GetPlatformById(id);

        if (platformItem is null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<PlatformReadDto>(platformItem));
    }

    [HttpPost]
    public async Task<ActionResult<PlatformReadDto>> CreatePlatform(
        PlatformCreateDto platformCreateDto
    )
    {
        Console.WriteLine("--> Creating Platform {0}", platformCreateDto);

        var platformModel = _mapper.Map<Platform>(platformCreateDto);
        await _repository.CreatePlatform(platformModel);
        _repository.SaveChanges();

        var platformReadDto = _mapper.Map<PlatformReadDto>(platformModel);
        return CreatedAtRoute(
            nameof(GetPlatformById),
            new { Id = platformReadDto.Id },
            platformReadDto
        );
    }
}
