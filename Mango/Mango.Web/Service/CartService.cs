using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service;

public class CartService(IBaseService baseService) : ICartService
{
    private readonly IBaseService _baseService = baseService;

    public async Task<ResponseDto?> ApplyCouponAsync(CartDto cartDto)
    {
        return await _baseService.SendAsync(new RequestDto
        { 
            ApiType = SD.ApiType.POST,
            Data = cartDto,
            Url = $"{SD.ShoppingCartApiBase}/api/cart/applycoupon"
        });
    }

    public async Task<ResponseDto?> EmailCartAsync(CartDto cartDto)
    {
        return await _baseService.SendAsync(new RequestDto
        {
            ApiType = SD.ApiType.POST,
            Data = cartDto,
            Url = $"{SD.ShoppingCartApiBase}/api/cart/email"
        });
    }

    public async Task<ResponseDto?> GetCartByUserIdAsync(string userId)
    {
        return await _baseService.SendAsync(new RequestDto
        {
            ApiType = SD.ApiType.GET,
            Url = $"{SD.ShoppingCartApiBase}/api/cart/{userId}"
        });
    }

    public async Task<ResponseDto?> RemoveFromCartAsync(int cartDetailsId)
    {
        return await _baseService.SendAsync(new RequestDto
        {
            ApiType = SD.ApiType.POST,
            Data = cartDetailsId,
            Url = $"{SD.ShoppingCartApiBase}/api/cart/remove"
        });
    }

    public async Task<ResponseDto?> UpsertCartAsync(CartDto cartDto)
    {
        return await _baseService.SendAsync(new RequestDto
        {
            ApiType = SD.ApiType.POST,
            Data = cartDto,
            Url = $"{SD.ShoppingCartApiBase}/api/cart/upsert"
        });
    }
}
