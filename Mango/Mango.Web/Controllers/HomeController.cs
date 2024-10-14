using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Mango.Web.Controllers;

public class HomeController(IProductService productService) : Controller
{
    private readonly IProductService _productService = productService;

    public async Task<IActionResult> Index()
    {
        var model = new List<ProductDto?>();

        var response = await _productService.GetAllProductsAsync();

        if (response is not null && response.IsSuccess)
        {
            model = JsonConvert.DeserializeObject<List<ProductDto?>>(Convert.ToString(response.Result)!);
        }
        else
        {
            TempData["error"] = response?.Message;
        }

        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> ProductDetails(int productId)
    {
        var model = new ProductDto();

        var response = await _productService.GetProductByIdAsync(productId);

        if (response is not null && response.IsSuccess)
        {
            model = JsonConvert.DeserializeObject<ProductDto?>(Convert.ToString(response.Result)!);
        }
        else
        {
            TempData["error"] = response?.Message;
        }

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
