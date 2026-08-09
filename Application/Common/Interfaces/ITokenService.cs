using Domain.Entities.UserEntity;

namespace Application.Common.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user, IList<string> roles);
}