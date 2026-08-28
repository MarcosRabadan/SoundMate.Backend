using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using SoundMate.API.Filters;
using SoundMate.API.Middleware;
using SoundMate.Application;
using SoundMate.Infrastructure;
using SoundMate.Infrastructure.Agendia;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("SoundMate")!);
builder.Services.AddAgendiaIntegration(builder.Configuration);

// Registered globally so no endpoint has to remember to validate its input.
builder.Services
    .AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(options =>
    {
        // Enums travel by name, not by number: "SoloTeacher" instead of 2. The numeric values are
        // a storage detail (explicit, so reordering cannot corrupt data) and the HTTP contract
        // should not inherit them. Numbers are still accepted on the way in, so nothing that
        // already sends them breaks.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// AddProblemDetails supplies the IProblemDetailsService the handler writes through, so every
// error leaves as the same RFC 7807 shape.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// First in the pipeline: it can only catch what happens after it.
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // MapOpenApi only serves the JSON document; Scalar is the UI on top of it.
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("SoundMate API"));
}

// Inside a container the app listens on HTTP only: the ASP.NET dev certificate is not there
// and TLS is the reverse proxy's job. Redirecting would bounce every request to a port nobody
// is listening on. DOTNET_RUNNING_IN_CONTAINER is set by the official .NET base images.
if (!app.Configuration.GetValue<bool>("DOTNET_RUNNING_IN_CONTAINER"))
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
