namespace ShopEase.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = [];

    private Order() { }

    public Order(
        string orderNumber,
        string customerName,
        string email,
        string phone,
        string shippingAddress,
        string city,
        string postalCode)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException(
                "Order number is required.", nameof(orderNumber));

        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException(
                "Customer name is required.", nameof(customerName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "Email is required.", nameof(email));

        if (string.IsNullOrWhiteSpace(shippingAddress))
            throw new ArgumentException(
                "Shipping address is required.", nameof(shippingAddress));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException(
                "City is required.", nameof(city));

        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException(
                "Postal code is required.", nameof(postalCode));

        OrderNumber = orderNumber;
        CustomerName = customerName.Trim();
        Email = email.Trim();
        Phone = phone?.Trim() ?? string.Empty;
        ShippingAddress = shippingAddress.Trim();
        City = city.Trim();
        PostalCode = postalCode.Trim();
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string ShippingAddress { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items;

    public decimal Total => _items.Sum(item => item.Subtotal);

    public void AddItem(
        int productId,
        string productName,
        decimal unitPrice,
        int quantity)
    {
        if (_items.Any(item => item.ProductId == productId))
            throw new InvalidOperationException(
                "A product can appear only once in an order.");

        _items.Add(new OrderItem(
            productId, productName, unitPrice, quantity));
    }
}
