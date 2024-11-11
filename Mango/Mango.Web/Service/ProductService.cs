using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service;

public class ProductService(IBaseService baseService) : IProductService
{
    private readonly IBaseService _baseService = baseService;

    public async Task<ResponseDto?> CreateProductAsync(ProductDto productDto)
    {
        return await _baseService.SendAsync(new RequestDto
        {
            ApiType = SD.ApiType.POST,
            Url = $"{SD.ProductApiBase}/api/product",
            Data = productDto,
            ContentType = SD.ContentType.MultipartFormData
        });
    }

    public async Task<ResponseDto?> DeleteProductAsync(int id)
    {
        return await _baseService.SendAsync(new RequestDto
        {
            ApiType = SD.ApiType.DELETE,
            Url = $"{SD.ProductApiBase}/api/product/{id}"
        });
    }

    public async Task<ResponseDto?> GetAllProductsAsync()
    {
        return await _baseService.SendAsync(new RequestDto
        {
            ApiType = SD.ApiType.GET,
            Url = $"{SD.ProductApiBase}/api/product"
        });
    }

    public async Task<ResponseDto?> GetProductByIdAsync(int id)
    {
        return await _baseService.SendAsync(new RequestDto
        {
            ApiType = SD.ApiType.GET,
            Url = $"{SD.ProductApiBase}/api/product/{id}"
        });
    }

    public async Task<ResponseDto?> UpdateProductAsync(ProductDto productDto)
    {
        return await _baseService.SendAsync(new RequestDto
        {
            ApiType = SD.ApiType.PUT,
            Url = $"{SD.ProductApiBase}/api/product",
            Data = productDto,
            ContentType = SD.ContentType.MultipartFormData
        });
    }
}
