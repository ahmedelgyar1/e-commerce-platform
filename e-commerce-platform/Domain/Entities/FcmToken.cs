using e_commerce_platform.Domain.Enums;

namespace e_commerce_platform.Domain.Entities;

public class FcmToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DeviceToken { get; set; } = string.Empty;
    public DeviceType DeviceType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}
