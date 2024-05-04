using Microsoft.EntityFrameworkCore;
using Order.API.Contexts;
using Common.Extensions;
using EventBus.RabbitMQ.Extensions;
using Order.API.IntegrationEvents.EventHandlers;
using Order.API.IntegrationEvents.InEvents;

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
builder.Services.AddScoped<PaymentCompletedEventHandler>();
builder.Services.AddScoped<PaymentFailedEventHandler>();
builder.Services.AddScoped<StockNotReservedEventHandler>();

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

app.AddEvent<PaymentCompletedEvent, PaymentCompletedEventHandler>();
app.AddEvent<PaymentFailedEvent, PaymentFailedEventHandler>();
app.AddEvent<StockNotReservedEvent, StockNotReservedEventHandler>();

app.Run();
