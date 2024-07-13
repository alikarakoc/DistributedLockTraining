using Dapper;
using DistributedLockTraining.Domain.Common;
using DistributedLockTraining.Infrastructure.Context;
using MediatR;

namespace DistributedLockTraining.Application.Features.Products.Queries
{
    public class GetSellableStockQuery : IRequest<ActionResponse<int>>
    {
        public int Id { get; set; }
    }
    public class GetSellableStockQueryHandler : IRequestHandler<GetSellableStockQuery, ActionResponse<int>>
    {
        #region Properties
        private readonly DapperDbContext _context;

        public GetSellableStockQueryHandler(DapperDbContext context)
        {
            _context = context;
        }
        #endregion
        public async Task<ActionResponse<int>> Handle(GetSellableStockQuery request, CancellationToken cancellationToken)
        {
            await using var connection = _context.GetConnection();
            var response = new ActionResponse<int>();
            response.IsSuccessful = true;
            var query = await connection.QueryFirstAsync<int>("select totalstock from products with(nolock) where productId = @id", new { id = request.Id });
            response.Data = query;
            return response;
        }
    }
}
