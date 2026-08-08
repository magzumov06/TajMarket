using Domain.DTOs.UserDto;

namespace Application.Features.User;

internal static class UserMapper
{
    public static UserProfileDto ToDto(Domain.Entities.UserEntity.User user, IList<string> roles) => new(
        user.Id,
        user.FullName,
        user.Email!,
        user.PhoneNumber,
        user.AvatarUrl,
        roles.ToList()
    );
}