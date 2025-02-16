﻿using AutoMapper;
using DistributedLockTraining.Application.Features.Products.Queries;
using DistributedLockTraining.Domain.Common;
using DistributedLockTraining.Domain.Entities;
using DistributedLockTraining.Infrastructure.Abstract;
using MediatR;

namespace DistributedLockTraining.Application.Features.Products.Commands
{
    public class CreateOrderCommand : IRequest<ActionResponse<bool>>
    {
        public required int ProductId { get; set; }
        public required int Quantity { get; set; }
    }
    public class CreateOrderCommandCommandHandler(
        IGenericRepository<OrderEntity> _orderRepository,
        IGenericRepository<ProductEntity> _productRepository,
        IMapper mapper, IMediator _mediator,
        IRedisService redisService)
        : IRequestHandler<CreateOrderCommand, ActionResponse<bool>>
    {
        private static readonly Random random = new Random();
        public async Task<ActionResponse<bool>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var response = new ActionResponse<bool>
            {
                ResponseType = ResponseType.Ok,
                IsSuccessful = true,
                Data = true,
            };
            if (request.Quantity <= 0)
            {
                response.ResponseType = ResponseType.Error;
                response.Errors = new List<string>() { "Stok 0 büyük olmalıdır!" };
                response.IsSuccessful = false;
                return response;
            }
            var lockKey = $"lock:product:{request.ProductId}";
            var lockValue = Guid.NewGuid().ToString();
            var lockExpiration = TimeSpan.FromSeconds(30);
            var acquired = await redisService.AcquireLockAsync(lockKey, lockValue, lockExpiration);
            if (!acquired)
            {
                response.Errors = new List<string>() { "Kaynak kilitli! Tutarlılık sağlandı!" };
                Console.WriteLine($"Kaynak kilitli! Tutarlılık sağlandı! {DateTime.Now}");
                response.IsSuccessful = false;
                return response;
            }

            try
            {
                var stockQuery = new GetSellableStockQuery { Id = request.ProductId };
                var stockResponse = await _mediator.Send(stockQuery, cancellationToken);
                var product = await _productRepository.GetAsync(request.ProductId);
                if (stockResponse.Data >= request.Quantity)
                {
                    if (request.Quantity <= product.MaxStockQuantity)
                    {
                        var productMapper = mapper.Map<OrderEntity>(request);
                        productMapper.OrderNumber = GenerateRandomString();
                        productMapper.OrderTotal = product.Price * request.Quantity;
                        var createProduct = await _orderRepository.AddAsync(productMapper);

                        if (createProduct == null)
                        {
                            response.ResponseType = ResponseType.Error;
                            response.IsSuccessful = false;
                            response.Data = false;
                        }
                        else
                        {
                            var stockUpdateQuery = new UpdateProductStockCommand { ProductId = request.ProductId, Quantity = request.Quantity };
                            var stockUpdate = await _mediator.Send(stockUpdateQuery, cancellationToken);
                            Console.WriteLine($"Stok güncellendi. {DateTime.Now}");
                        }
                    }
                    else
                    {
                        response.IsSuccessful = false;
                        response.Data = false;
                        response.Errors = new List<string>() { "Maksimum stok satış sınırı aşıldı!" };
                        Console.WriteLine($"Maksimum stok satış sınırı aşıldı Talep : {request.Quantity} Kalan Stok : {product.TotalStock} {DateTime.Now}");
                    }
                }
                else
                {
                    response.IsSuccessful = false;
                    response.Data = false;
                    response.Errors = new List<string>() { "Yetersiz stok." };
                    Console.WriteLine($"Yetersiz stok. Talep : {request.Quantity} Kalan Stok : {product.TotalStock} {DateTime.Now}");
                }

                return response;
            }
            finally
            {
                await redisService.ReleaseLockAsync(lockKey, lockValue);
            }
        }

        private static string GenerateRandomString()
        {
            const int length = 6;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] stringChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            return new string(stringChars);
        }
    }
}
