using Domain.Entities;
using Domain.Entities.UserEntity;

namespace Infrastructure.Auth;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user, IList<string> roles);
}