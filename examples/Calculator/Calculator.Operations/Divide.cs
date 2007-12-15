using System;
using Calculator.Api;

namespace Calculator.Operations
{
    public class Divide : IOperation
    {
        INotifier _notifier;
        int _places;

        public Divide(INotifier notifier, int places)
        {
            if (notifier == null)
                throw new ArgumentNullException("notifier");

            _notifier = notifier;
            _places = places;
        }

        #region IOperation Members

        public string Operator
        {
            get
            {
                return "/";
            }
        }

        public double Apply(double lhs, double rhs)
        {
            if (rhs == 0.0)
            {
                _notifier.Notify("Cannot divide by zero.");
                return Double.NaN;
            }

            return Math.Round(lhs / rhs, _places);
        }

        #endregion
    }
}
