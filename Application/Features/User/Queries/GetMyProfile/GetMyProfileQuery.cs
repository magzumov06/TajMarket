using Domain.DTOs.UserDto;
using Domain.Responses;
using MediatR;

namespace Application.Features.User.Queries.GetMyProfile;

public class GetMyProfileQuery(int userId) : IRequest<Response<UserProfileDto>>
{
    public int UserId { get; } = userId;
}