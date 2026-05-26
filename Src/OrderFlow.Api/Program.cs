using OrderFlow.Api;
using OrderFlow.Api.Endpoints.Pokemon;
using OrderFlow.Application;
using OrderFlow.Domain;
using OrderFlow.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    });

    builder.Services.AddOpenApi();

    var pokeApiBaseUrl = builder.Configuration["ToxiProxy:BaseUrl"]
        ?? builder.Configuration["PokeApi:BaseUrl"]
        ?? "https://pokeapi.co";

    builder.Services
        .AddDomain()
        .AddApplication()
        .AddInfrastructure(pokeApiBaseUrl)
        .AddApi();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
        app.MapOpenApi();

    app.UseApiMiddlewares();
    app.UseHttpsRedirection();

    app.MapPokemonEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação encerrada inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
