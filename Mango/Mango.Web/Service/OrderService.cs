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
