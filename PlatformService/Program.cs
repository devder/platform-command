using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PlatformService.Data;
using PlatformService.Profiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
var configuration = builder.Configuration;
var environment = builder.Environment;

if (environment.IsProduction())
{
    Console.WriteLine("--> Using SqlServer Db");
    // builder.Services.AddDbContext<AppDbContext>(opt =>
    //     opt.UseSqlServer(configuration.GetConnectionString("PlatformsConn")));

    // TODO: Remove
    Console.WriteLine("--> Using InMem Db Temporarily");
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMem"));
}
else
{
    Console.WriteLine("--> Using InMem Db");
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMem"));
}

// DI registrations
builder.Services.AddScoped<IPlatformRepo, PlatformRepo>(); // register this for dependency injection

// builder.Services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();
// builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();
// builder.Services.AddGrpc();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<PlatformsProfile>();
});

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PlatformService", Version = "v1" });
});

Console.WriteLine($"--> CommandService Endpoint {configuration["CommandService"]}");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PlatformService v1"));
}

// app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapGet(
    "/protos/platforms.proto",
    async context =>
    {
        await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
    }
);

app.MapGet("/hello", () => "Hello World!");

// app.UseEndpoints(endpoints =>
// {
//     endpoints.MapControllers();
//     // endpoints.MapGrpcService<GrpcPlatformService>();

//     endpoints.MapGet(
//         "/protos/platforms.proto",
//         async context =>
//         {
//             await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
//         }
//     );
// });
app.MapControllers();

await PrepDb.MigrateDbAsync(app);

app.Run();

// https://github.com/binarythistle/S04E03---.NET-Microservices-Course-/blob/main/PlatformService/Startup.cs
