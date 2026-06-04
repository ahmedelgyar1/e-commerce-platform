using e_commerce_platform.Domain.Enums;

namespace e_commerce_platform.Domain.Entities;

public class Merchant : ApplicationUser
{
    public Merchant()
    {
        Role = UserRole.Merchant;
    }

    public ICollection<Product> Products { get; set; } = [];
}
