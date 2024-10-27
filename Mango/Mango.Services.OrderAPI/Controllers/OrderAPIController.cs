using AutoMapper;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dtos;
using Mango.Services.OrderAPI.Service.IService;
using Mango.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
}
