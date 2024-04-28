using Microsoft.EntityFrameworkCore;
using Common.Extensions;
using EventBus.RabbitMQ.Extensions;
using Stock.API.Contexts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("StockDb");
});

builder.Services.ConfigureCQRSServices();

builder.AddRabbitMQEventBus();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Hello");

using var scope = app.Services.CreateScope();

var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

context.Stocks.AddRange(new List<Stock.API.Models.Stock>
{
    new()
    {
        Count = 10,
        Id = 1,
        ProductId = 1
    },
    new()
    {
        Count = 15,
        Id = 2,
        ProductId = 2
    },
    new()
    {
        Count = 20,
        Id = 3,
        ProductId = 3
    },
    new()
    {
        Count = 25,
        Id = 4,
        ProductId = 4
    },
    new()
    {
        Count = 30,
        Id = 5,
        ProductId = 5
    },
    new()
    {
        Count = 35,
        Id = 6,
        ProductId = 6
    },
    new()
    {
        Count = 40,
        Id = 7,
        ProductId = 7
    },
    new()
    {
        Count = 45,
        Id = 8,
        ProductId = 8
    },
    new()
    {
        Count = 50,
        Id = 9,
        ProductId = 9
    },
    new()
    {
        Count = 55,
        Id = 10,
        ProductId = 10
    }
});

app.Run();
