using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service;

public class OrderService(IBaseService baseService) : IOrderService
{
    private readonly IBaseService _baseService = baseService;

    public async Task<ResponseDto?> CreateOrderAsync(CartDto cartDto)
    {
        return await _baseService.SendAsync(new RequestDto
        {
            ApiType = SD.ApiType.POST,
            Url = $"{SD.OrderApiBase}/api/order/create",
            Data = cartDto
        });
    }

    public async Task<ResponseDto?> CreateStripeSessionAsync(StripeRequestDto stripeRequestDto)
    {
        return await _baseService.SendAsync(new RequestDto
        {
            ApiType = SD.ApiType.POST,
            Url = $"{SD.OrderApiBase}/api/order/create-stripe-session",
            Data = stripeRequestDto
        });
    }

    public async Task<ResponseDto?> GetAllOrdersAsync(string? userId)
    {
        return await _baseService.SendAsync(new RequestDto
        {
            ApiType = SD.ApiType.GET,
            Url = $"{SD.OrderApiBase}/api/order/get-orders?userId={userId}"
        });
    }

    public async Task<ResponseDto?> GetOrderAsync(int orderId)
    {
        return await _baseService.SendAsync(new RequestDto
        {
            ApiType = SD.ApiType.GET,
            Url = $"{SD.OrderApiBase}/api/order/get-order/{orderId}"            
        });
    }

    public async Task<ResponseDto?> UpdateOrderStatusAsync(int orderId, string newStatus)
    {
        return await _baseService.SendAsync(new RequestDto
        {
            ApiType = SD.ApiType.POST,
            Url = $"{SD.OrderApiBase}/api/order/update-status/{orderId}",
            Data = newStatus
        });
    }

    public async Task<ResponseDto?> ValidateStripeSessionAsync(int orderHeaderId)
    {
        return await _baseService.SendAsync(new RequestDto
        {
            ApiType = SD.ApiType.POST,
            Url = $"{SD.OrderApiBase}/api/order/validate-stripe-session",
            Data = orderHeaderId
        });
    }
}
