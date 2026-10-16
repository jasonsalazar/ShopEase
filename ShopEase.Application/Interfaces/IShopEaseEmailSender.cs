namespace ShopEase.Application.Interfaces;

public interface IShopEaseEmailSender
{
    Task SendAsync(
        string email,
        string subject,
        string htmlMessage,
        CancellationToken cancellationToken = default);
}
