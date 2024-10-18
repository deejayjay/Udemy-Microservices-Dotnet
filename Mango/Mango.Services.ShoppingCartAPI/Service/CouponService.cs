using Mango.Services.ShoppingCartAPI.Models.Dtos;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Newtonsoft.Json;

namespace Mango.Services.ShoppingCartAPI.Service;

public class CouponService(IHttpClientFactory httpClientFactory) : ICouponService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<CouponDto> GetCouponByCodeAsync(string couponCode)
    {
        var client = _httpClientFactory.CreateClient("Coupon");

        var response = await client.GetAsync($"/api/coupon/code/{couponCode}");
        var apiContet = await response.Content.ReadAsStringAsync();

        var resp = JsonConvert.DeserializeObject<ResponseDto>(apiContet);

        if (resp is not null && resp.IsSuccess)
        {
            return JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(resp.Result)!)!;
        }
        return new CouponDto();
    }
}
