namespace ShopEase.Application.DTOs;

public class OrderListItemDto
{
    public int Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal Total { get; init; }
}

public class OrderManagementPageDto
{
    public IReadOnlyList<OrderListItemDto> Items { get; init; }
        = [];

    public string? Search { get; init; }
    public string? Status { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }

    public int TotalPages =>
        (int)Math.Ceiling(TotalCount / (double)PageSize);
}
