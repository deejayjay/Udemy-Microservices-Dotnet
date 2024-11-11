using AutoMapper;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ProductAPI.Controllers;

//[Authorize]
[Route("api/product")]
[ApiController]
public class ProductAPIController(AppDbContext db, IMapper mapper) : ControllerBase
{
    private readonly AppDbContext _db = db;
    private readonly IMapper _mapper = mapper;

    private ResponseDto _response = new();

    [HttpGet]
    public async Task<ResponseDto> GetAllAsync()
    {
        try
        {
            var productsList = await _db.Products.ToListAsync();

            _response.Result = _mapper.Map<IEnumerable<ProductDto>>(productsList);
        }
        catch (Exception e)
        {
            _response.IsSuccess = false;
            _response.Message = e.Message;
        }

        return _response;
    }

    [HttpGet("{id:int}")]
    public async Task<ResponseDto> GetByIdAsync(int id)
    {
        try
        {
            var product = await _db.Products.FirstAsync(p => p.ProductId == id);

            _response.Result = _mapper.Map<ProductDto>(product);
        }
        catch (Exception e)
        {
            _response.IsSuccess = false;
            _response.Message = e.Message;
        }

        return _response;
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public async Task<ResponseDto> CreateProductAsync(ProductDto productDto)
    {
        try
        {
            var product = _mapper.Map<Product>(productDto);

            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();

            if (productDto.Image is not null)
            {
                var fileName = product.ProductId + Path.GetExtension(productDto.Image.FileName);
                var filePath = $@"wwwroot\ProductImages\{fileName}";
                var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                using var fileStream = new FileStream(filePathDirectory, FileMode.Create);
                await productDto.Image.CopyToAsync(fileStream);

                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                product.ImageUrl = baseUrl + "/ProductImages/" + fileName;
                product.ImageLocalPath = filePath;
            }
            else
            {
                product.ImageUrl = "https://placehold.co/600x400";
            }

            _db.Products.Update(product);
            await _db.SaveChangesAsync();

            _response.Result = _mapper.Map<ProductDto>(product);
        }
        catch (Exception e)
        {
            _response.IsSuccess = false;
            _response.Message = e.Message;
        }

        return _response;
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut]
    public async Task<ResponseDto> UpdateProductAsync(ProductDto productDto)
    {
        try
        {
            var product = _mapper.Map<Product>(productDto);

            if (productDto.Image is not null)
            {
                // Delete old image.
                if (!string.IsNullOrEmpty(product.ImageLocalPath))
                {
                    var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), product.ImageLocalPath);
                    FileInfo file = new(oldFilePathDirectory);
                    if (file.Exists)
                    {
                        file.Delete();
                    }
                }

                var fileName = product.ProductId + Path.GetExtension(productDto.Image.FileName);
                var filePath = $@"wwwroot\ProductImages\{fileName}";
                var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                using var fileStream = new FileStream(filePathDirectory, FileMode.Create);
                await productDto.Image.CopyToAsync(fileStream);

                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                product.ImageUrl = baseUrl + "/ProductImages/" + fileName;
                product.ImageLocalPath = filePath;
            }
            else
            {
                product.ImageUrl = "https://placehold.co/600x400";
            }

            _db.Products.Update(product);
            await _db.SaveChangesAsync();

            _response.Result = _mapper.Map<ProductDto>(product);
        }
        catch (Exception e)
        {
            _response.IsSuccess = false;
            _response.Message = e.Message;
        }

        return _response;
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id:int}")]
    public async Task<ResponseDto> DeleteProductAsync(int id)
    {
        try
        {
            var existingProduct = await _db.Products.FirstAsync(p => p.ProductId == id);

            if (!string.IsNullOrEmpty(existingProduct.ImageLocalPath)) 
            {
                var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), existingProduct.ImageLocalPath);
                FileInfo file = new (oldFilePathDirectory);
                if(file.Exists)
                {
                    file.Delete();
                }
            }

            _db.Products.Remove(existingProduct);
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _response.IsSuccess = false;
            _response.Message = e.Message;
        }

        return _response;
    }
}
