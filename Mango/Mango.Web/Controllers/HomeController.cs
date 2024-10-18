using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers;

public class HomeController(IProductService productService, ICartService cartService) : Controller
{
    private readonly IProductService _productService = productService;
    private readonly ICartService _cartService = cartService;

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

    [Authorize]
    [HttpPost]
    [ActionName(nameof(ProductDetails))]
    public async Task<IActionResult> ProductDetails(ProductDto productDto)
    {
        var cartDto = new CartDto
        {
            CartHeader = new CartHeaderDto
            {
                UserId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub).FirstOrDefault()?.Value!
            },
            CartDetails = 
            [
                new CartDetailsDto 
                {
                    ProductId = productDto.ProductId,
                    Count = productDto.Count
                }
            ]
        };

        var response = await _cartService.UpsertCartAsync(cartDto);

        if (response is not null && response.IsSuccess)
        {
            TempData["success"] = "Item has been added to the cart.";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            TempData["error"] = response?.Message;
        }

        return View(productDto);
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
