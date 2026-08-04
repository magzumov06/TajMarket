using Application.Features.Category.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Category.Queries.GetCategoryById;

public class GetCategoryByIdQuery(int id) : IRequest<Response<CategoryDto>>
{
    public int Id { get; } = id;
}