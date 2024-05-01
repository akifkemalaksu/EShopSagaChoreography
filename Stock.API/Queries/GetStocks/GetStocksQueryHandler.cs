using Common.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Stock.API.Contexts;
using Stock.API.Dtos;

namespace Stock.API.Queries.GetStocks
{
    public class GetStocksQueryHandler : IQueryHandler<GetStocksQuery, GetStocksQueryResult>
    {
        private readonly AppDbContext _dbContext;

        public GetStocksQueryHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetStocksQueryResult> Handle(GetStocksQuery query, CancellationToken token)
        {
            var stocks = await _dbContext.Stocks.ProjectToType<StockDto>().ToListAsync();
            return new GetStocksQueryResult
            {
                Stocks = stocks
            };
        }
    }
}
