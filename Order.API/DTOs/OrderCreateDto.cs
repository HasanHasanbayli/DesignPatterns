namespace Order.API.DTOs;

public class OrderCreateDto
{
    public string BuyerId { get; set; }
    public List<OrderItemDto> OrderItem { get; set; }
    public PaymentDto Payment { get; set; }
    public AddressDto Address { get; set; }
}