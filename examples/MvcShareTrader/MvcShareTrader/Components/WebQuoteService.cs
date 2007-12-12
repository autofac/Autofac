using System;
using MvcShareTrader.Services;
using System.Diagnostics;

namespace MvcShareTrader.Components
{
    public class WebQuoteService : IQuoteService, IDisposable
    {
        public WebQuoteService()
        {
            Trace.WriteLine("Creating quote service.");
        }

        #region IQuoteService Members

        public decimal GetQuote(string symbol)
        {
            return (decimal)symbol[0];
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Trace.WriteLine("Disposing quote service.");
        }

        #endregion
    }
}
