using e_commerce_platform.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace e_commerce_platform.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string? OtpCode { get; set; }
    public DateTime? OtpExpiry { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
