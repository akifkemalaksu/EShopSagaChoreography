using Stock.API.Dtos;

namespace Stock.API.Queries.GetStocks
{
    public class GetStocksQueryResult
    {
        public IEnumerable<StockDto> Stocks { get; set; }
    }
}
