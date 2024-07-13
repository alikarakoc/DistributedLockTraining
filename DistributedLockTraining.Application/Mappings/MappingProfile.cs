using AutoMapper;
using DistributedLockTraining.Application.Features.Products.Commands;
using DistributedLockTraining.Domain.Entities;

namespace DistributedLockTraining.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateProductCommand, ProductEntity>().ReverseMap();
            CreateMap<CreateOrderCommand, OrderEntity>().ReverseMap();
        }
    }
}
