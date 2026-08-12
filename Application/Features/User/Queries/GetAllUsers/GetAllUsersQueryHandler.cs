using System.Net;
using Application.Common.Interfaces;
using Application.Features.User.DTOs;
using Domain.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.User.Queries.GetAllUsers;

public class GetAllUsersQueryHandler(
    IApplicationDbContext context,
    ILogger<GetAllUsersQueryHandler> logger)
    : IRequestHandler<GetAllUsersQuery, PaginationResponse<List<UserProfileDto>>>
{
    public async Task<PaginationResponse<List<UserProfileDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        try
        {
            logger.LogInformation("Retrieving all users with filter {@Filter}", filter);

            var query = context.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                query = query.Where(u =>
                    u.FullName.Contains(filter.SearchTerm) ||
                    (u.Email != null && u.Email.Contains(filter.SearchTerm)));

            if (filter.IsActive.HasValue)
                query = query.Where(u => u.IsActive == filter.IsActive.Value);
            
            if (!string.IsNullOrWhiteSpace(filter.Role))
                query = query.Where(u => context.UserRoles
                    .Any(ur => ur.UserId == u.Id &&
                               context.Roles.Any(r => r.Id == ur.RoleId && r.Name == filter.Role)));

            query = query.OrderByDescending(u => u.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var users = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            if (users.Count == 0)
            {
                return new PaginationResponse<List<UserProfileDto>>(
                    new List<UserProfileDto>(), totalCount, filter.PageNumber, filter.PageSize);
            }

            var userIds = users.Select(u => u.Id).ToList();

            var roleRows = await (
                from ur in context.UserRoles
                join r in context.Roles on ur.RoleId equals r.Id
                where userIds.Contains(ur.UserId)
                select new { ur.UserId, RoleName = r.Name! }
            ).ToListAsync(cancellationToken);

            var rolesByUser = roleRows
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).ToList());

            var result = users
                .Select(user => UserMapper.ToDto(
                    user,
                    rolesByUser.TryGetValue(user.Id, out var roles) ? roles : new List<string>()))
                .ToList();

            logger.LogInformation("Retrieved {UserCount} users, total {TotalCount}", result.Count, totalCount);

            return new PaginationResponse<List<UserProfileDto>>(result, totalCount, filter.PageNumber, filter.PageSize);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting all users");
            return new PaginationResponse<List<UserProfileDto>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}