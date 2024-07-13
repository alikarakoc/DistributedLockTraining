using AutoMapper;
using DistributedLockTraining.Domain.Common;
using DistributedLockTraining.Domain.Entities;
using DistributedLockTraining.Infrastructure.Abstract;
using MediatR;

namespace DistributedLockTraining.Application.Features.Products.Commands
{
    public class CreateProductCommand : IRequest<ActionResponse<bool>>
    {
        public required string Name { get; set; }
        public required int TotalStock { get; set; }
        public required int MaxStockQuantity { get; set; }
        public required decimal Price { get; set; }
    }
    public class CreateProductCommandCommandHandler(
        IGenericRepository<ProductEntity> _productRepository,
        IMapper mapper)
        : IRequestHandler<CreateProductCommand, ActionResponse<bool>>
    {
        public async Task<ActionResponse<bool>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var response = new ActionResponse<bool>
            {
                ResponseType = ResponseType.Ok,
                IsSuccessful = true,
                Data = true,
            };
            var productMapper = mapper.Map<ProductEntity>(request);
            var createProduct = await _productRepository.AddAsync(productMapper);
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
