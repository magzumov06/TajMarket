using Domain.Responses;
using MediatR;

namespace Application.Features.Product.Commands.DeleteProduct;

public class DeleteProductCommand(int sellerUserId, int productId) : IRequest<Response<string>>
{
    public int SellerUserId { get; } = sellerUserId;
    public int ProductId { get; } = productId;
}