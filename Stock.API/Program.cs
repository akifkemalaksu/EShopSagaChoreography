using Microsoft.EntityFrameworkCore;
using Common.Extensions;
using EventBus.RabbitMQ.Extensions;
using Stock.API.Contexts;
using Stock.API.IntegrationEvents.EventHandlers;
using Stock.API.IntegrationEvents.InEvents;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
}, contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Transient);

builder.Services.ConfigureCQRSServices();

builder.AddRabbitMQEventBus();
builder.Services.AddTransient<OrderCreatedEventHandler>();

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
if (!context.Stocks.Any())
{
    context.Stocks.AddRange(new List<Stock.API.Models.Stock>
    {
        new()
        {
            Count = 10,
            ProductId = 1
        },
        new()
        {
            Count = 15,
            ProductId = 2
        },
        new()
        {
            Count = 20,
            ProductId = 3
        },
        new()
        {
            Count = 25,
            ProductId = 4
        },
        new()
        {
            Count = 30,
            ProductId = 5
        },
        new()
        {
            Count = 35,
            ProductId = 6
        },
        new()
        {
            Count = 40,
            ProductId = 7
        },
        new()
        {
            Count = 45,
            ProductId = 8
        },
        new()
        {
            Count = 50,
            ProductId = 9
        },
        new()
        {
            Count = 55,
            ProductId = 10
        }
    });

    context.SaveChanges();
}

app.AddEvent<OrderCreatedEvent, OrderCreatedEventHandler>();

app.Run();
