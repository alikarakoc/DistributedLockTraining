using DistributedLockTraining.Domain.Common;
using DistributedLockTraining.Domain.Entities;
using DistributedLockTraining.Infrastructure.Abstract;
using MediatR;

namespace DistributedLockTraining.Application.Features.Products.Commands
{
    public class UpdateProductStockCommand : IRequest<ActionResponse<bool>>
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
    public class UpdateProductStockCommandCommandHandler(
        IGenericRepository<ProductEntity> _productRepository
        )
        : IRequestHandler<UpdateProductStockCommand, ActionResponse<bool>>
    {
        public async Task<ActionResponse<bool>> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
        {
            var response = new ActionResponse<bool>
            {
                ResponseType = ResponseType.Ok,
                IsSuccessful = true,
                Data = true,
            };

            var product = await _productRepository.GetAsync(request.ProductId);
            product.TotalStock -= request.Quantity;

            var createProduct = await _productRepository.UpdateAsync(product);
            if (createProduct == null)
            {
                response.ResponseType = ResponseType.Error;
                response.IsSuccessful = false;
                response.Data = false;
            }
            return response;
        }
    }
}
