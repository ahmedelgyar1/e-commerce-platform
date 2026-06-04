using e_commerce_platform.Domain.Entities;

namespace e_commerce_platform.Domain.Interfaces;

public interface IProductRepository : IBaseRepository<Product>
{
    IQueryable<Product> GetProductsQueryable();
    Task<Product?> GetProductWithMerchantAsync(Guid id);
}
