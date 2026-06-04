namespace e_commerce_platform.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public Guid? ParentCategoryId { get; set; }  
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;  
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Category? ParentCategory { get; set; }             
    public ICollection<Category> SubCategories { get; set; } = [];  
    public ICollection<ProductCategory> ProductCategories { get; set; } = [];
}
