using e_commerce_platform.Infrastructure.Data;
using e_commerce_platform.Application.Interfaces;
using e_commerce_platform.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using AttributeEntity = e_commerce_platform.Domain.Entities.Attribute;

namespace e_commerce_platform.Infrastructure.Repositories;

public class AttributeRepository : BaseRepository<AttributeEntity>, IAttributeRepository
{
    public AttributeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<AttributeEntity?> GetWithValuesAsync(Guid id)
    {
        return await Context.Attributes
            .Include(a => a.Values)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<AttributeEntity>> GetByProductIdAsync(Guid productId)
    {
        return await Context.Attributes
            .Include(a => a.Values)
            .Where(a => a.ProductId == productId)
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNameAsync(Guid productId, string name)
    {
        var nameLower = name.ToLower();
        return await Context.Attributes
            .AnyAsync(a => a.ProductId == productId && a.Name.ToLower() == nameLower);
    }
}
