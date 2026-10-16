using System.ComponentModel.DataAnnotations;

namespace ShopEase.Web.Models.Account;

public class ForgotPasswordViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
