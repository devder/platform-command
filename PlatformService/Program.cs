using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Profiles;
using PlatformService.SyncDataServices.Grpc;
using PlatformService.SyncDataServices.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
var configuration = builder.Configuration;
var environment = builder.Environment;

if (environment.IsProduction())
{
    Console.WriteLine("--> Using SqlServer Db");
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlServer(configuration.GetConnectionString("PlatformsConn"))
    );
}
else
{
    Console.WriteLine("--> Using InMem Db");
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMem"));
}

// DI registrations
builder.Services.AddScoped<IPlatformRepo, PlatformRepo>(); // register this for dependency injection
builder.Services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();

builder.Services.AddGrpc();
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Add gRPC Reflection services (only in Development)
builder.Services.AddGrpcReflection();
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.ListenAnyIP(80, listenOptions => listenOptions.Protocols = HttpProtocols.Http1); // plain HTTP/1.1
    options.ListenAnyIP(90, listenOptions => listenOptions.Protocols = HttpProtocols.Http2); // plain HTTP/2
});

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
app.MapGrpcService<GrpcPlatformService>();
if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

// this is optional, used to serve the proto file to the client
app.MapGet(
    "/protos/platforms.proto",
    async context =>
    {
        await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
    }
);

await app.MigrateDbAsync();

app.Run();

// https://github.com/binarythistle/S04E03---.NET-Microservices-Course-/blob/main/PlatformService/Startup.cs
