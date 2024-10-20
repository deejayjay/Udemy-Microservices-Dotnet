using Microsoft.AspNetCore.Mvc;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Mango.Web.Models;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;

namespace Mango.Web.Controllers;
public class CartController(ICartService cartService) : Controller
{
    private readonly ICartService _cartService = cartService;

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var cart = await LoadCartDtoBasedOnLoggedInUserAsync();

        return View(cart);
    }

    public async Task<IActionResult> Remove(int cartDetailsId)
    {
        var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub).FirstOrDefault()?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            return View();

        var response = await _cartService.RemoveFromCartAsync(cartDetailsId);

        if (response is not null && response.IsSuccess)
        {
            TempData["success"] = "Cart updated successfully";
            return RedirectToAction(nameof(Index));
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
    {
        var response = await _cartService.ApplyCouponAsync(cartDto);

        if (response is not null && response.IsSuccess)
        {
            TempData["success"] = "Cart updated successfully";            
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
    {
        var response = await _cartService.ApplyCouponAsync(cartDto);

        if (response is not null && response.IsSuccess)
        {
            TempData["success"] = "Coupon removed successfully";
            return RedirectToAction(nameof(Index));
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> EmailCart(CartDto _)
    {
        var cart = await LoadCartDtoBasedOnLoggedInUserAsync();
        cart.CartHeader.Email = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Email).FirstOrDefault()?.Value;

        var response = await _cartService.EmailCartAsync(cart);

        if (response is not null && response.IsSuccess)
        {
            TempData["success"] = "You should receive a copy of the cart in email shortly.";
        }

        return RedirectToAction(nameof(Index));
    }

    #region Helper Methods
    
    private async Task<CartDto> LoadCartDtoBasedOnLoggedInUserAsync()
    {
        var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub).FirstOrDefault()?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            return new CartDto();

        var response = await _cartService.GetCartByUserIdAsync(userId);

        if (response is null || !response.IsSuccess)
            return new CartDto();

        var cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result)!);

        return cartDto ?? new CartDto();
    } 

    #endregion
}
