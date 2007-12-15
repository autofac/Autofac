using Calculator.Api;

namespace Calculator.Operations
{
    public class Add : IOperation
    {
        #region IOperation Members

        public string Operator
        {
            get
            {
                return "+";
            }
        }

        public double Apply(double lhs, double rhs)
        {
            return lhs + rhs;
        }

        #endregion
    }
}
