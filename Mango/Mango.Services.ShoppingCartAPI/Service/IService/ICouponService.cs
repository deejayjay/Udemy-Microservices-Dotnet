using Mango.Services.ShoppingCartAPI.Models.Dtos;

namespace Mango.Services.ShoppingCartAPI.Service.IService;

public interface ICouponService
{
    Task<CouponDto> GetCouponByCodeAsync(string couponCode);
}
