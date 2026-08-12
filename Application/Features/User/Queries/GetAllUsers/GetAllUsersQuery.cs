using Application.Features.User.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.User.Queries.GetAllUsers;

public class GetAllUsersQuery(UserFilter filter) : IRequest<PaginationResponse<List<UserProfileDto>>>
{
    public UserFilter Filter { get; set; } = filter;
}