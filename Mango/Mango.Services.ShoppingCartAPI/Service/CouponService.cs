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
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var couponResponse = JsonConvert.DeserializeObject<ResponseDto>(content);

            if (couponResponse is not null && couponResponse.IsSuccess)
            {
                return JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(couponResponse.Result)!) ?? new CouponDto();
            }
        }

        return new CouponDto(); ;
    }
}
