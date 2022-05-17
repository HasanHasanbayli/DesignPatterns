using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.API.Consumers;
using Stock.API.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedEventConsumer>();

    x.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(builder.Configuration.GetConnectionString("RabbitMQ"));

        configurator.ReceiveEndpoint(RabbitMQSettingsConst.StockOrderCreatedEventQueueName,
            e =>
            {
                e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
            });
    });
});

builder.Services.AddMassTransitHostedService();

builder.Services.AddDbContext<ApplicationDbContext>(optionsBuilder =>
{
    optionsBuilder.UseInMemoryDatabase("StockDb");
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    context.Stocks.Add(new Stock.API.Models.Stock {Id = 1, ProductId = 1, Count = 100});
    context.Stocks.Add(new Stock.API.Models.Stock {Id = 2, ProductId = 2, Count = 100});

    context.SaveChanges();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();