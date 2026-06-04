using e_commerce_platform.Domain.Entities;

namespace e_commerce_platform.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user);
    string GenerateRefreshToken();
}
