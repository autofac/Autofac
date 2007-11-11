using ShareTrader.Services;
using System.Diagnostics;
using System;

namespace ShareTrader.Components
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
