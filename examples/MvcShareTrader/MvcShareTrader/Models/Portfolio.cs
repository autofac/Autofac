using System.Collections.Generic;
using System.Linq;

namespace MvcShareTrader.Models
{
    public class Portfolio
    {
        Shareholding.Factory ShareholdingFactory { get; set; }
        IList<Shareholding> _holdings = new List<Shareholding>();

        public Portfolio(Shareholding.Factory shareholdingFactory)
        {
            ShareholdingFactory = shareholdingFactory;
        }

        public void Add(string symbol, uint holding)
        {
            _holdings.Add(ShareholdingFactory(symbol, holding));
        }

        public decimal Value
        {
            get
            {
                return _holdings.Aggregate(0m, (a, e) => a + e.Value);
            }
        }
    }
}
