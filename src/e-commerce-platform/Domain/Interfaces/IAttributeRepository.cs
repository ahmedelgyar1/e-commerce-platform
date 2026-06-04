using AttributeEntity = e_commerce_platform.Domain.Entities.Attribute;

namespace e_commerce_platform.Domain.Interfaces;

public interface IAttributeRepository : IBaseRepository<AttributeEntity>
{
    Task<AttributeEntity?> GetWithValuesAsync(Guid id);
    Task<List<AttributeEntity>> GetByProductIdAsync(Guid productId);
    Task<bool> ExistsByNameAsync(Guid productId, string name);
}
