using AutoInsight.Yards;
using AutoInsight.Vehicles;
using AutoInsight.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using AutoInsight.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi("v2");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
                o => o.MapEnum<VehicleModel>("vehicle_model")
                )
        .UseSnakeCaseNamingConvention());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.AddDocument("v2", "API v2", "/openapi/v2.json", isDefault: true);
    });
}

app.MapGroup("/v2")
    .MapYardEnpoints()
    .MapVehicleEnpoints();

app.Run();
