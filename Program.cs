using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using ShopAPI.Data;
using ShopAPI.Extensions;
using ShopAPI.Factory;
using ShopAPI.Repositories;
using ShopAPI.Services;
using ShopAPI.Singleton;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options => 
options.UseSqlServer(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")));

// Patterns

// Register Repository
builder.Services.AddRepositories();
// Register Service
builder.Services.AddServices();
// Singleton
builder.Services.AddAppConfiguration();
// Factory
builder.Services.AddFactories();
// Observer
builder.Services.AddObservers();
// Adapter
builder.Services.AddAdapters();


// API
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();

// Subscribe observers
app.SubscribeObservers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
