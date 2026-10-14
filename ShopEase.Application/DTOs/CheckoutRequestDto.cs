using System.ComponentModel.DataAnnotations;

namespace ShopEase.Application.DTOs;

public class CheckoutRequestDto
{
    [Required, StringLength(100)]
    [Display(Name = "Full name")]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(250)]
    [Display(Name = "Shipping address")]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [Display(Name = "Postal code")]
    public string PostalCode { get; set; } = string.Empty;
}
