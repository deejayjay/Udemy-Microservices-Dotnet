﻿namespace Mango.Services.OrderAPI.Models.Dtos;

public class OrderHeaderDto
{
    public int OrderHeaderId { get; set; }
    public string UserId { get; set; }
    public string? CouponCode { get; set; }
    public decimal Discount { get; set; }
    public decimal OrderTotal { get; set; }
    public IEnumerable<OrderDetailsDto> OrderDetails { get; set; }

    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public DateTime OrderTime { get; set; }
    public string? Status { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? StripeSessionId { get; set; }
}
