namespace MvcShareTrader.Services
{
    public interface IQuoteService
    {
        decimal GetQuote(string symbol);
    }
}
