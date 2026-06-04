using e_commerce_platform.Domain.Entities;

namespace e_commerce_platform.Domain.Interfaces;

public interface IProductVariantRepository : IBaseRepository<ProductVariant>
{
    Task<ProductVariant?> GetWithValuesAsync(Guid id);
    Task<ProductVariant?> GetBySkuAsync(string sku);
    IQueryable<ProductVariant> GetVariantsQueryable(Guid productId);
    Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeVariantId = null);
}
