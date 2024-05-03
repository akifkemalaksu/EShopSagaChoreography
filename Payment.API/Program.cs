using EventBus.RabbitMQ.Extensions;
using Common.Extensions;
using Payment.API.IntegrationEvents.InEvents;
using Payment.API.IntegrationEvents.EventHandlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureCQRSServices();

builder.AddRabbitMQEventBus();
builder.Services.AddScoped<StockReservedEventHandler>();

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

app.AddEvent<StockReservedEvent, StockReservedEventHandler>();

app.Run();
