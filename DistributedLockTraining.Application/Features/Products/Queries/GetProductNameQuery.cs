using Dapper;
using DistributedLockTraining.Domain.Common;
using DistributedLockTraining.Infrastructure.Context;
using MediatR;

namespace DistributedLockTraining.Application.Features.Products.Queries
{
    public class GetProductNameQuery : IRequest<ActionResponse<int>>
    {
        public required string ProductName { get; set; }
    }
    public class GetProductNameQueryHandler : IRequestHandler<GetProductNameQuery, ActionResponse<int>>
    {
        #region Properties
        private readonly DapperDbContext _context;

        public GetProductNameQueryHandler(DapperDbContext context)
        {
            _context = context;
        }
        #endregion
        public async Task<ActionResponse<int>> Handle(GetProductNameQuery request, CancellationToken cancellationToken)
        {
            await using var connection = _context.GetConnection();
            var response = new ActionResponse<int>();
            response.IsSuccessful = true;
            var query = await connection.QueryFirstAsync<int>("select count(name) from products with(nolock) where name = @productname", new { productname = request.ProductName });
            response.Data = query;
            return response;
        }
    }
}
