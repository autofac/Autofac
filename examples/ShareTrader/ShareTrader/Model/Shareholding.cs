
using ShareTrader.Services;
using System;
namespace ShareTrader.Model
{
    public class Shareholding
    {
        public delegate Shareholding Factory(string symbol, uint holding);

        IQuoteService QuoteService { get; set; }

        public Shareholding(string symbol, uint holding, IQuoteService quoteService)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentException("Invalid symbol.");

            QuoteService = quoteService;
            Symbol = symbol;
            Holding = holding;
        }

        public string Symbol { get; private set; }

        public uint Holding { get; set; }

        public decimal Value
        {
            get
            {
                return QuoteService.GetQuote(Symbol) * Holding;
            }
        }
    }
}
