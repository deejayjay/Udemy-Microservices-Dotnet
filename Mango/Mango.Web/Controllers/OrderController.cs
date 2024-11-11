using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers;
public class OrderController(IOrderService orderService) : Controller
{
    private readonly IOrderService _orderService = orderService;

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> OrderDetails(int orderId)
    {
        var orderHeaderDto = new OrderHeaderDto();

        var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
        var response = await _orderService.GetOrderAsync(orderId);

        if (response != null && response.IsSuccess)
        {
            orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result)!)!;
        }

        if(!User.IsInRole(SD.RoleAdmin) && userId != orderHeaderDto.UserId)
        {
            return NotFound();
        }

        return View(orderHeaderDto);
    }

    [HttpPost("OrderReadyForPickup")]
    public async Task<IActionResult> OrderReadyForPickup(int orderId)
    {
        var response = await _orderService.UpdateOrderStatusAsync(orderId, SD.Status_ReadyForPickup);

        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Status updated successfully";
            return RedirectToAction(nameof(OrderDetails), new { orderId });
        }

        return View();
    }

    [HttpPost("CompleteOrder")]
    public async Task<IActionResult> CompleteOrder(int orderId)
    {
        var response = await _orderService.UpdateOrderStatusAsync(orderId, SD.Status_Completed);

        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Status updated successfully";
            return RedirectToAction(nameof(OrderDetails), new { orderId });
        }

        return View();
    }

    [HttpPost("CancelOrder")]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        var response = await _orderService.UpdateOrderStatusAsync(orderId, SD.Status_Cancelled);

        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Status updated successfully";
            return RedirectToAction(nameof(OrderDetails), new { orderId });
        }

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(string status)
    {
        IEnumerable<OrderHeaderDto> ordersList = [];

        var userId = !User.IsInRole(SD.RoleAdmin)
            ? User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value
            : null;

        var response = await _orderService.GetAllOrdersAsync(userId);

        if (response != null && response.IsSuccess)
        {
            ordersList = JsonConvert.DeserializeObject<List<OrderHeaderDto>>(Convert.ToString(response.Result)!)!;

            switch (status)
            {
                case "approved":
                    ordersList = ordersList.Where(o => o.Status == SD.Status_Approved);
                    break;

                case "readyforpickup":
                    ordersList = ordersList.Where(o => o.Status == SD.Status_ReadyForPickup);
                    break;

                case "cancelled":
                    ordersList = ordersList.Where(o => o.Status == SD.Status_Cancelled);
                    break;                

                default:
                    break;
            }
        }
        
        return Json(new { data = ordersList });
    }
}
