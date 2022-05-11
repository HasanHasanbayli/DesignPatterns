using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Order.API.Context;
using Order.API.DTOs;
using Order.API.Models;
using Shared;

namespace Order.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrdersController(ApplicationDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost]
    public async Task<IActionResult> Create(OrderCreateDto orderCreateDto)
    {
        Models.Order newOrder = new()
        {
            BuyerId = orderCreateDto.BuyerId,
            Status = OrderStatus.Suspend,
            Address = new Address
            {
                Line = orderCreateDto.Address.Line,
                Province = orderCreateDto.Address.Province,
                District = orderCreateDto.Address.District
            },
            CreatedDate = DateTime.Now
        };

        orderCreateDto.OrderItem.ForEach(dto =>
        {
            newOrder.Items.Add(new OrderItem {Price = dto.Price, ProductId = dto.ProductId, Count = dto.Count});
        });

        await _context.Orders.AddAsync(newOrder);

        await _context.SaveChangesAsync();

        OrderCreatedEvent orderCreatedEvent = new()
        {
            BuyerId = orderCreateDto.BuyerId,
            OrderId = newOrder.Id,
            Payment = new PaymentMessage
            {
                CardName = orderCreateDto.Payment.CardName,
                CardNumber = orderCreateDto.Payment.CardNumber,
                Expiration = orderCreateDto.Payment.Expiration,
                CVV = orderCreateDto.Payment.CVV,
                TotalPrice = orderCreateDto.OrderItem.Sum(x => x.Price * x.Count)
            }
        };

        orderCreateDto.OrderItem.ForEach(dto =>
        {
            orderCreatedEvent.OrderItems.Add(new OrderItemMessage
            {
                Count = dto.Count,
                ProductId = dto.ProductId
            });
        });

        await _publishEndpoint.Publish(orderCreatedEvent);
        
        return Ok();
    }
}