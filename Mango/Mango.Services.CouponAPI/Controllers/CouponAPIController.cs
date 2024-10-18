using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mango.Services.CouponAPI.Data;
using Mango.Services.CouponAPI.Models;
using Mango.Services.CouponAPI.Models.Dtos;

namespace Mango.Services.CouponAPI.Controllers;

[Route("api/coupon")]
[ApiController]
[Authorize]
public class CouponAPIController(AppDbContext db, IMapper mapper) : ControllerBase
{
    private readonly AppDbContext _db = db;
    private readonly IMapper _mapper = mapper;

    private ResponseDto _response = new();

    [HttpGet]
    public async Task<ResponseDto> GetAllAsync()
    {
        try
        {
            var couponsList = await _db.Coupons.ToListAsync();

            _response.Result = _mapper.Map<IEnumerable<CouponDto>>(couponsList);
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
            var coupon = await _db.Coupons.FirstAsync(c => c.CouponId == id);

            _response.Result = _mapper.Map<CouponDto>(coupon);
        }
        catch (Exception e)
        {
            _response.IsSuccess = false;
            _response.Message = e.Message;
        }

        return _response;
    }

    [HttpGet("code/{code}")]
    public async Task<ResponseDto> GetByCodeAsync(string code)
    {
        try
        {
            var coupon = await _db.Coupons.FirstAsync(c => c.CouponCode.ToLower() == code.ToLower());

            _response.Result = _mapper.Map<CouponDto>(coupon);
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
    public async Task<ResponseDto> CreateCouponAsync([FromBody] CouponDto couponDto)
    {
        try
        {
            var coupon = _mapper.Map<Coupon>(couponDto);

            await _db.Coupons.AddAsync(coupon);
            await _db.SaveChangesAsync();

            _response.Result = _mapper.Map<CouponDto>(coupon);
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
    public async Task<ResponseDto> UpdateCouponAsync([FromBody] CouponDto couponDto)
    {
        try
        {
            var coupon = _mapper.Map<Coupon>(couponDto);

            _db.Coupons.Update(coupon);
            await _db.SaveChangesAsync();

            _response.Result = _mapper.Map<CouponDto>(coupon);
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
    public async Task<ResponseDto> DeleteCouponAsync(int id)
    {
        try
        {
            var existingCoupon = await _db.Coupons.FirstAsync(c => c.CouponId == id);

            _db.Coupons.Remove(existingCoupon);
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
