using Scalar.AspNetCore;
using SoundMate.Infrastructure;
using SoundMate.Infrastructure.Agendia;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("SoundMate")!);
builder.Services.AddAgendiaIntegration(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // MapOpenApi only serves the JSON document; Scalar is the UI on top of it.
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("SoundMate API"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
