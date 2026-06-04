using e_commerce_platform.Domain.Entities;
using e_commerce_platform.Infrastructure.Data;
using e_commerce_platform.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace e_commerce_platform.Infrastructure.Repositories;

public class AttributeValueRepository : BaseRepository<AttributeValue>, IAttributeValueRepository
{
    public AttributeValueRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByValueAsync(Guid attributeId, string value)
    {
        var valLower = value.ToLower();
        return await Context.AttributeValues
            .AnyAsync(av => av.AttributeId == attributeId && av.Value.ToLower() == valLower);
    }
}
