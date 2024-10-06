using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers;

public class CouponController(ICouponService couponService) : Controller
{
    private readonly ICouponService _couponService = couponService;

    public async Task<IActionResult> Index()
    {
        var coupons = new List<CouponDto?>();

        var response = await _couponService.GetAllCouponsAsync();

        if(response is not null && response.IsSuccess)
        {
            coupons = JsonConvert.DeserializeObject<List<CouponDto?>>(Convert.ToString(response.Result));
        }
        else
        {
            TempData["error"] = response?.Message;
        }

        return View(coupons);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CouponDto model)
    {
        if(ModelState.IsValid) 
        {
            var response = await _couponService.CreateCouponAsync(model);

            if (response is not null && response.IsSuccess)
            {
                TempData["success"] = "Coupon created successfully";
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
    public async Task<IActionResult> Delete(int couponId)
    {
        var response = await _couponService.GetCouponByIdAsync(couponId);

        if (response is not null && response.IsSuccess)
        {
            var couponDto = JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(response.Result));
            
            return View(couponDto);
        }
        else
        {
            TempData["error"] = response?.Message;
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Delete(CouponDto couponDto)
    {
        var response = await _couponService.DeleteCouponAsync(couponDto.CouponId);

        if (response is not null && response.IsSuccess)
        {
            TempData["success"] = "Coupon deleted successfully";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            TempData["error"] = response?.Message;
        }

        return View(couponDto);
    }
}
