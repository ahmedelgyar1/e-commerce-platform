using e_commerce_platform.Domain.Enums;

namespace e_commerce_platform.Domain.Entities;

public class Customer : ApplicationUser
{
    public Customer()
    {
        Role = UserRole.Customer;
    }

    public ICollection<Address> Addresses { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
}
