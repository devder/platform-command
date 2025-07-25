using System.Text.Json;
using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;

namespace CommandService.EventProcessing;

public class EventProcessor(IServiceScopeFactory serviceScopeFactory, IMapper mapper)
    : IEventProcessor
{
    public async Task ProcessEvent(string message)
    {
        var eventType = DetermineEvent(message);
        switch (eventType)
        {
            case EventType.PlatformPublished:
                await AddPlatformAsync(message);
                break;
            default:
                break;
        }
    }

    static EventType DetermineEvent(string notificationMessage)
    {
        Console.WriteLine("--> Determining Event");
        var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);

        switch (eventType?.Event)
        {
            case "Platform_Published":
                Console.WriteLine("--> Platform Published Event Detected");
                return EventType.PlatformPublished;
            default:
                Console.WriteLine("--> Could not determine the event type");
                return EventType.Undetermined;
        }
    }

    async Task AddPlatformAsync(string platformPublishedMessage)
    {
        using var scope = serviceScopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();
        var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(
            platformPublishedMessage
        );
        try
        {
            var platform = mapper.Map<Platform>(platformPublishedDto);
            if (!await repo.ExternalPlatformExists(platform.ExternalId))
            {
                await repo.CreatePlatform(platform);
                repo.SaveChanges();
                Console.WriteLine("--> New Platform added");
            }
            else
            {
                Console.WriteLine("--> Platform already exists");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not add Platform to DB {ex.Message}");
        }
    }
}
