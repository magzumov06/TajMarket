using Application.Features.Category.DTOs;
using Domain.Responses;
using MediatR;

namespace Application.Features.Category.Commands.CreateCategory;

public class CreateCategoryCommand(CreateCategoryDto dto) : IRequest<Response<string>>
{
    public CreateCategoryDto Dto { get; } = dto;
}