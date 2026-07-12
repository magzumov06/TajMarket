using Domain.DTOs.AuthDto;
using Domain.Responses;

namespace Infrastructure.Auth;

public interface IAuthService
{
    Task<Response<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<Response<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<Response<string>> ChangePassword(ChangePassword changePassword, int userId);

}
