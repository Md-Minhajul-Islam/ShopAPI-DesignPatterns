using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using ShopAPI.Data;
using ShopAPI.Factory;
using ShopAPI.Repositories;
using ShopAPI.Services;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options => 
options.UseSqlServer(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")));

// Register Repository
builder.Services.AddScoped<IProductRepositories, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();


// Register Service
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();


// Register Factory Pattern
builder.Services.AddSingleton<IPaymentProcessorFactory, PaymentProcessorFactory>();


builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
