using Domain.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.User.Commands.UploadAvatar;

public class UploadAvatarCommand(int userId, IFormFile file) : IRequest<Response<string>>
{
    public int UserId { get; } = userId;
    public IFormFile File { get; } = file;
}