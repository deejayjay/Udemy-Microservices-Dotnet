using Mango.Services.ShoppingCartAPI.Models.Dtos;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Newtonsoft.Json;


namespace Mango.Services.ShoppingCartAPI.Service;

public class ProductService(IHttpClientFactory httpClientFactory) : IProductService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var client = _httpClientFactory.CreateClient("Product");

        var response = await client.GetAsync("/api/product");
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {            
            var productsResponse = JsonConvert.DeserializeObject<ResponseDto>(content);

            if (productsResponse is not null && productsResponse.IsSuccess) 
            {
                return JsonConvert.DeserializeObject<IEnumerable<ProductDto>>(Convert.ToString(productsResponse.Result)!)
                       ?? [];
            }
        }

        return [];
    }
}
