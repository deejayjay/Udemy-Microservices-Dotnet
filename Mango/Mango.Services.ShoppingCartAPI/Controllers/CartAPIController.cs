using AutoMapper;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dtos;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Mango.Services.ShoppingCartAPI.Controllers;
[Route("api/cart")]
[ApiController]
public class CartAPIController(AppDbContext db, IMapper mapper, IProductService productService, ICouponService couponService) : ControllerBase
{
    private readonly AppDbContext _db = db;
    private readonly IMapper _mapper = mapper;
    private readonly IProductService _productService = productService;
    private readonly ICouponService _couponService = couponService;

    private readonly ResponseDto _response = new();

    [HttpGet("{userId}")]
    public async Task<ResponseDto> GetCartAsync(string userId)
    {
        try
        {
            CartDto? cart = null;

            var cartHeaderFromDb = await _db.CartHeaders.FirstOrDefaultAsync(ch => ch.UserId == userId);

            cart = new()
            {
                CartHeader = _mapper.Map<CartHeaderDto>(cartHeaderFromDb),
            };

            if (cartHeaderFromDb is not null)
            {
                var cartDetailsFromDb = _db.CartDetails.Where(cd => cd.CartHeaderId == cartHeaderFromDb!.CartHeaderId);

                cart.CartDetails = _mapper.Map<IEnumerable<CartDetailsDto>>(cartDetailsFromDb);

                var allProducts = await _productService.GetAllProductsAsync();

                foreach (var item in cart.CartDetails)
                {
                    item.Product = allProducts.FirstOrDefault(p => p.ProductId == item.ProductId);
                    cart.CartHeader.CartTotal += item.Count * item.Product?.Price ?? 0m;
                }

                // Apply coupon if any
                if (!string.IsNullOrWhiteSpace(cart.CartHeader.CouponCode))
                {
                    var coupon = await _couponService.GetCouponByCodeAsync(cart.CartHeader.CouponCode);

                    if (coupon is not null && cart.CartHeader.CartTotal > coupon.MinAmount)
                    {
                        cart.CartHeader.CartTotal -= (decimal)coupon.DiscountAmount;
                        cart.CartHeader.Discount = (decimal)coupon.DiscountAmount;
                    }
                }
            }
            _response.Result = cart;
        }
        catch (Exception ex)
        {
            _response.Message = ex.Message;
            _response.IsSuccess = false;
        }

        return _response;
    }

    [HttpPost("applycoupon")]
    public async Task<object> ApplyCouponAsync([FromBody] CartDto cartDto)
    {
        try
        {
            var cartFromDb = await _db.CartHeaders.FirstAsync(u => u.UserId == cartDto.CartHeader.UserId);
            cartFromDb.CouponCode = cartDto.CartHeader.CouponCode;

            _db.CartHeaders.Update(cartFromDb);
            await _db.SaveChangesAsync();

            _response.Result = true;
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.ToString();
        }
        return _response;
    }

    [HttpPost("upsert")]
    public async Task<ResponseDto> UpsertAsync(CartDto cartDto)
    {
        try
        {
            var cartHeaderFromDb = await _db.CartHeaders
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(ch => ch.UserId == cartDto.CartHeader.UserId);

            var cartDetailsDto = cartDto.CartDetails!.First();

            if (cartHeaderFromDb is null)
            {
                // Create CartHeader and CartDetails.
                CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);

                await _db.CartHeaders.AddAsync(cartHeader);
                await _db.SaveChangesAsync();

                cartDetailsDto.CartHeaderId = cartHeader.CartHeaderId;

                CartDetails cartDetails = _mapper.Map<CartDetails>(cartDetailsDto);

                await _db.CartDetails.AddAsync(cartDetails);
                await _db.SaveChangesAsync();
            }
            else
            {
                // Check if CartDetails has the same product.
                var cartDetailsFromDb = await _db.CartDetails
                                                    .AsNoTracking()
                                                    .FirstOrDefaultAsync(cd => cd.ProductId == cartDetailsDto.ProductId
                                                                                        && cd.CartHeaderId == cartHeaderFromDb.CartHeaderId);

                if (cartDetailsFromDb is null)
                {
                    // Create CartDetails.
                    cartDetailsDto.CartHeaderId = cartHeaderFromDb.CartHeaderId;

                    CartDetails cartDetails = _mapper.Map<CartDetails>(cartDetailsDto);

                    await _db.CartDetails.AddAsync(cartDetails);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    // Update count in CartDetails.
                    cartDetailsDto.Count += cartDetailsFromDb.Count;

                    cartDetailsDto.CartHeaderId = cartDetailsFromDb.CartHeaderId;
                    cartDetailsDto.CartDetailsId = cartDetailsFromDb.CartDetailsId;

                    CartDetails cartDetails = _mapper.Map<CartDetails>(cartDetailsDto);

                    _db.CartDetails.Update(cartDetails);
                    await _db.SaveChangesAsync();
                }
            }

            _response.Result = cartDto;
        }
        catch (Exception ex)
        {
            _response.Message = ex.Message;
            _response.IsSuccess = false;
        }

        return _response;
    }

    [HttpPost("remove")]
    public async Task<ResponseDto> RemoveFromCartAsync([FromBody] int cartDetailsId)
    {
        try
        {
            var cartDetailsFromDb = await _db.CartDetails.FirstAsync(cd => cd.CartDetailsId == cartDetailsId);

            var totalCountOfCartItems = _db.CartDetails.Where(cd => cd.CartHeaderId == cartDetailsFromDb.CartHeaderId).Count();

            _db.CartDetails.Remove(cartDetailsFromDb);

            if (totalCountOfCartItems == 1)
            {
                var cartHeaderToRemove = await _db.CartHeaders.FirstOrDefaultAsync(ch => ch.CartHeaderId == cartDetailsFromDb.CartHeaderId);

                if (cartHeaderToRemove is not null)
                    _db.CartHeaders.Remove(cartHeaderToRemove);
            }

            await _db.SaveChangesAsync();

            _response.Result = true;
        }
        catch (Exception ex)
        {
            _response.Message = ex.Message;
            _response.IsSuccess = false;
        }

        return _response;
    }
}
