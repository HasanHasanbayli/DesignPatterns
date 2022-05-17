using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.API.Context;

namespace Stock.API.Consumers;

public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly ILogger<OrderCreatedEventConsumer> _logger;
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderCreatedEventConsumer(ApplicationDbContext applicationDbContext,
        ILogger<OrderCreatedEventConsumer> logger, ISendEndpointProvider sendEndpointProvider,
        IPublishEndpoint publishEndpoint)
    {
        _applicationDbContext = applicationDbContext;
        _logger = logger;
        _sendEndpointProvider = sendEndpointProvider;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var stockResult = new List<bool>();

        foreach (var item in context.Message.OrderItems)
        {
            stockResult.Add(
                await _applicationDbContext.Stocks.AnyAsync(x =>
                    x.ProductId == item.ProductId && x.Count > item.Count));
        }

        if (stockResult.All(x => x.Equals(true)))
        {
            foreach (var item in context.Message.OrderItems)
            {
                var stock = await _applicationDbContext.Stocks.FirstOrDefaultAsync(x => x.ProductId == item.ProductId);

                if (stock != null)
                {
                    stock.Count -= item.Count;
                }

                await _applicationDbContext.SaveChangesAsync();
            }

            _logger.LogInformation($"Stock for reserved for Buyer Id: {context.Message.BuyerId}");

            var sendEndPoint =
                await _sendEndpointProvider.GetSendEndpoint(
                    new Uri($"queue:{RabbitMQSettingsConst.StockReservedEventQueueName}"));

            StockReservedEvent stockReservedEvent = new()
            {
                Payment = context.Message.Payment,
                BuyerId = context.Message.BuyerId,
                OrderId = context.Message.OrderId,
                OrderItems = context.Message.OrderItems
            };

            await sendEndPoint.Send(stockReservedEvent);
        }
        else
        {
            await _publishEndpoint.Publish(new StockNotReservedEvent
            {
                OrderId = context.Message.OrderId,
                Message = "Not enough stock"
            });
        }
    }
}