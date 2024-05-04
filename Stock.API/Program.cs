using Microsoft.EntityFrameworkCore;
using Common.Extensions;
using EventBus.RabbitMQ.Extensions;
using Stock.API.Contexts;
using Stock.API.IntegrationEvents.EventHandlers;
using Stock.API.IntegrationEvents.InEvents;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.ConfigureCQRSServices();

builder.AddRabbitMQEventBus();
builder.Services.AddScoped<OrderCreatedEventHandler>();
builder.Services.AddScoped<PaymentFailedEventHandler>();

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

app.AddEvent<OrderCreatedEvent, OrderCreatedEventHandler>();
app.AddEvent<PaymentFailedEvent, PaymentFailedEventHandler>();

using var scope = app.Services.CreateScope();

var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
if (!context.Stocks.Any())
{
    context.Stocks.AddRange(new List<Stock.API.Models.Stock>
    {
        new()
        {
            Count = 50,
            ProductId = 1
        },
        new()
        {
            Count = 50,
            ProductId = 2
        },
        new()
        {
            Count = 50,
            ProductId = 3
        },
        new()
        {
            Count = 50,
            ProductId = 4
        },
        new()
        {
            Count = 50,
            ProductId = 5
        },
        new()
        {
            Count = 50,
            ProductId = 6
        },
        new()
        {
            Count = 50,
            ProductId = 7
        },
        new()
        {
            Count = 50,
            ProductId = 8
        },
        new()
        {
            Count = 50,
            ProductId = 9
        },
        new()
        {
            Count = 50,
            ProductId = 10
        }
    });

    context.SaveChanges();
}

app.Run();
