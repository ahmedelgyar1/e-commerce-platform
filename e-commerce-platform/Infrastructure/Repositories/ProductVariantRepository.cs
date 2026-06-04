using e_commerce_platform.Domain.Entities;
using e_commerce_platform.Infrastructure.Data;
using e_commerce_platform.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace e_commerce_platform.Infrastructure.Repositories;

public class ProductVariantRepository : BaseRepository<ProductVariant>, IProductVariantRepository
{
    public ProductVariantRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<ProductVariant?> GetWithValuesAsync(Guid id)
    {
        return await Context.ProductVariants
            .Include(pv => pv.AttributeValues)
                .ThenInclude(pvav => pvav.AttributeValue)
            .FirstOrDefaultAsync(pv => pv.Id == id);
    }

    public async Task<ProductVariant?> GetBySkuAsync(string sku)
    {
        var skuLower = sku.ToLower();
        return await Context.ProductVariants
            .Include(pv => pv.AttributeValues)
                .ThenInclude(pvav => pvav.AttributeValue)
            .FirstOrDefaultAsync(pv => pv.SKU.ToLower() == skuLower);
    }

    public IQueryable<ProductVariant> GetVariantsQueryable(Guid productId)
    {
        return Context.ProductVariants
            .Include(pv => pv.AttributeValues)
                .ThenInclude(pvav => pvav.AttributeValue)
            .Where(pv => pv.ProductId == productId);
    }

    public async Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeVariantId = null)
    {
        var skuLower = sku.ToLower();
        var query = Context.ProductVariants.AsNoTracking();

        if (excludeVariantId.HasValue)
        {
            query = query.Where(pv => pv.Id != excludeVariantId.Value);
        }

        return !await query.AnyAsync(pv => pv.SKU.ToLower() == skuLower);
    }
}
