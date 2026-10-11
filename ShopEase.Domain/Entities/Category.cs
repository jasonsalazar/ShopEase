namespace ShopEase.Domain.Entities;

public class Category
{
    private Category() { }

    public Category(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.");

        Name = name.Trim();
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.");

        Name = name.Trim();
        Description = description;
    }
}