using Application.Features.Category.DTOs;
using MediatR;

namespace Application.Features.Category.Queries.GetCategoryTree;

public class GetCategoryTreeQuery : IRequest<List<CategoryDto>>;