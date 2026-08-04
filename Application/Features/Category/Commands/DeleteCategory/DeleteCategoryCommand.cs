using Domain.Responses;
using MediatR;

namespace Application.Features.Category.Commands.DeleteCategory;

public class DeleteCategoryCommand(int id) : IRequest<Response<string>>
{
    public int Id { get; } = id;
}