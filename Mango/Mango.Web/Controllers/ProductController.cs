using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers;
public class ProductController(IProductService productService) : Controller
{
    private readonly IProductService _productService = productService;

    [Route("/product")]
    [Route("/product/index")]
    [Route("/product/productindex")]
    public async Task<IActionResult> ProductIndex()
    {
        var products = new List<ProductDto?>();

        var response = await _productService.GetAllProductsAsync();

        if (response is not null && response.IsSuccess)
        {
            products = JsonConvert.DeserializeObject<List<ProductDto?>>(Convert.ToString(response.Result));
        }
        else
        {
            TempData["error"] = response?.Message;
        }

        return View(products);
    }

    [HttpGet]
    public IActionResult ProductCreate()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ProductCreate(ProductDto model)
    {
        if (ModelState.IsValid)
        {
            var response = await _productService.CreateProductAsync(model);

            if (response is not null && response.IsSuccess)
            {
                TempData["success"] = "Product created successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ProductDelete(int id)
    {
        var response = await _productService.GetProductByIdAsync(id);

        if (response is not null && response.IsSuccess)
        {
            var productDto = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));

            return View(productDto);
        }
        else
        {
            TempData["error"] = response?.Message;
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> ProductDelete(ProductDto productDto)
    {
        var response = await _productService.DeleteProductAsync(productDto.ProductId);

        if (response is not null && response.IsSuccess)
        {
            TempData["success"] = "Product deleted successfully";
            return RedirectToAction(nameof(ProductIndex));
        }
        else
        {
            TempData["error"] = response?.Message;
        }

        return View(productDto);
    }

    [HttpGet]
    public async Task<IActionResult> ProductEdit(int id)
    {
        var response = await _productService.GetProductByIdAsync(id);

		if (response is not null && response.IsSuccess)
		{
			var productDto = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));

			return View(productDto);
		}
		else
		{
			TempData["error"] = response?.Message;
		}

		return NotFound();
	}

    [HttpPost]
    public async Task<IActionResult> ProductEdit(ProductDto model)
    {
        if (ModelState.IsValid)
        {
            var response = await _productService.UpdateProductAsync(model);

            if (response is not null && response.IsSuccess)
            {
                TempData["success"] = "Product updated successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
        }

        return View(model);
    }
}
