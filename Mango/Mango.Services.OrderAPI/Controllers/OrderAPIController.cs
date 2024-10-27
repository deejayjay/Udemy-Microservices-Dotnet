using AutoMapper;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dtos;
using Mango.Services.OrderAPI.Service.IService;
using Mango.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace Mango.Services.OrderAPI.Controllers;
[Route("api/order")]
[ApiController]
public class OrderAPIController(AppDbContext db, IMapper mapper, IProductService productService) : ControllerBase
{
    protected ResponseDto _response = new();

    private readonly AppDbContext _db = db;
    private readonly IMapper _mapper = mapper;
    private readonly IProductService _productService = productService;

    [Authorize]
    [HttpPost("create")]
    public async Task<ResponseDto> CreateOrderAsync([FromBody] CartDto cartDto)
    {
        try
        {
            var orderHeaderDto = _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);

            orderHeaderDto.OrderTime = DateTime.Now;
            orderHeaderDto.Status = SD.Status_Pending;
            orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderDetailsDto>>(cartDto.CartDetails);

            var orderCreated = _mapper.Map<OrderHeader>(orderHeaderDto);

            await _db.OrderHeaders.AddAsync(orderCreated);
            await _db.SaveChangesAsync();

            orderHeaderDto.OrderHeaderId = orderCreated.OrderHeaderId;

            _response.Result = orderHeaderDto;
        }
        catch (Exception ex)
        {
            _response.Message = ex.Message;
            _response.IsSuccess = false;
        }

        return _response;
    }

    [Authorize]
    [HttpPost("create-stripe-session")]
    public async Task<ResponseDto> CreateStripeSessionAsync([FromBody] StripeRequestDto stripeRequestDto)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                LineItems = [],
                Mode = "payment",
                SuccessUrl = stripeRequestDto.ApprovedUrl,
                CancelUrl = stripeRequestDto.CancelUrl                
            };            

            foreach (var item in stripeRequestDto.OrderHeader.OrderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.ProductName                            
                        }
                    },
                    Quantity = item.Count
                };

                options.LineItems.Add(sessionLineItem);
            }

            List<SessionDiscountOptions> discounts = [new() { Coupon = stripeRequestDto.OrderHeader.CouponCode }];

            if (stripeRequestDto.OrderHeader.Discount > 0)
            {
                options.Discounts = discounts;
            }

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            stripeRequestDto.StripeSessionUrl = session.Url;
            stripeRequestDto.StripeSessionId = session.Id;
            
            OrderHeader orderHeader = await _db.OrderHeaders.FirstAsync(o => o.OrderHeaderId == stripeRequestDto.OrderHeader.OrderHeaderId);
            orderHeader.StripeSessionId = session.Id;

            await _db.SaveChangesAsync();

            _response.Result = stripeRequestDto;
        }
        catch (Exception ex)
        {
            _response.Message = ex.Message;
            _response.IsSuccess = false;
        }

        return _response;
    }

    [Authorize]
    [HttpPost("validate-stripe-session")]
    public async Task<ResponseDto> ValidateStripeSessionAsync([FromBody] int orderHeaderId)
    {
        try
        {
            var orderHeader = await _db.OrderHeaders.FirstAsync(o => o.OrderHeaderId == orderHeaderId);

            var service = new SessionService();
            var session = await service.GetAsync(orderHeader.StripeSessionId);

            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = await paymentIntentService.GetAsync(session.PaymentIntentId);

            if (paymentIntent.Status == "succeeded")
            {
                // Payment successful
                orderHeader.PaymentIntentId = paymentIntent.Id;
                orderHeader.Status = SD.Status_Approved;

                await _db.SaveChangesAsync();
            }            

            _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
        }
        catch (Exception ex)
        {
            _response.Message = ex.Message;
            _response.IsSuccess = false;
        }

        return _response;
    }
}
