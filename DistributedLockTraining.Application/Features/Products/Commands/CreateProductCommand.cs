using AutoMapper;
using DistributedLockTraining.Application.Features.Products.Queries;
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
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ActionResponse<bool>>
    {
        private readonly IGenericRepository<ProductEntity> _productRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private static readonly object _lock = new object();

        public CreateProductCommandHandler(
            IGenericRepository<ProductEntity> productRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ActionResponse<bool>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var response = new ActionResponse<bool>
            {
                ResponseType = ResponseType.Ok,
                IsSuccessful = true,
                Data = true,
            };


            /*
            
            lock object kullanarakta paralel gelen isteklerde ki mükerrer kayıt oluşumunu önleyebiliyoruz, fakat bazı durumlarda load balancer kullanılan yapılarda özellikle,
            distributed locking mekanizması daha sağlıklı olacaktır.
            distributed lock için Redisle birlikte gelen RedLockNet kütüphanesinden faydalanıyoruz. const içerisinde IDistributedLockFactory _lockFactory ekledikten sonra tüm kodumuzu;

            await using var _lock = await _lockFactory.CreateLockAsync($"Product_{request.ProductId}", TimeSpan.FromSeconds(5));
            if (_lock.IsAcquired)
            {
                bu kısımda yazıyoruz...
            }

             */

            lock (_lock)
            {
                var checkProductNameQuery = new GetProductNameQuery { ProductName = request.Name };
                var checkProductName = _mediator.Send(checkProductNameQuery, cancellationToken).Result;
                if (checkProductName.Data == 0)
                {
                    var productMapper = _mapper.Map<ProductEntity>(request);
                    var createProduct = _productRepository.AddAsync(productMapper).Result;

                    if (createProduct == null)
                    {
                        response.ResponseType = ResponseType.Error;
                        response.IsSuccessful = false;
                        response.Data = false;
                    }
                }
                else
                {
                    response.ResponseType = ResponseType.Error;
                    response.IsSuccessful = false;
                    response.Data = false;
                    response.Errors = new List<string> { "Aynı isimde bir ürün zaten var!" };
                }
            }
            return await Task.FromResult(response);
        }
    }
}
