using e_commerce_platform.Domain.Entities;

namespace e_commerce_platform.Domain.Interfaces;

public interface IAttributeValueRepository : IBaseRepository<AttributeValue>
{
    Task<bool> ExistsByValueAsync(Guid attributeId, string value);
}
