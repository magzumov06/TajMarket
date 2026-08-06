using Domain.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Product.Commands.UploadProductImages;

public class UploadProductImagesCommand(int sellerUserId, int productId, IEnumerable<IFormFile> files)
    : IRequest<Response<string>>
{
    public int SellerUserId { get; } = sellerUserId;
    public int ProductId { get; } = productId;
    public IEnumerable<IFormFile> Files { get; } = files;
}