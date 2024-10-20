﻿using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Models;

public class CartHeaderDto
{
    public int CartHeaderId { get; set; }
    public string UserId { get; set; }
    public string? CouponCode { get; set; }
    public decimal Discount { get; set; }
    public decimal CartTotal { get; set; }    
    [Required]
    public string? Name { get; set; }
    [Required]
    public string? Phone { get; set; }
    [Required]
    public string? Email { get; set; }
}
