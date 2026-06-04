using e_commerce_platform.Domain.Entities;
using e_commerce_platform.Infrastructure.Data;
using e_commerce_platform.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace e_commerce_platform.Infrastructure.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    public IQueryable<Product> GetProductsQueryable()
    {
        return Context.Products.AsQueryable();
    }

    public async Task<Product?> GetProductWithMerchantAsync(Guid id)
    {
        return await Context.Products
            .Include(p => p.Merchant)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
