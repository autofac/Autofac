using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Calculator.Api
{
    /// <summary>
    /// A (mathematical) operation that can be performed
    /// on two integers.
    /// </summary>
    public interface IOperation
    {
        /// <summary>
        /// The operator describing the operation, e.g. "+".
        /// </summary>
        string Operator
        {
            get;
        }

        /// <summary>
        /// Apply the operator to the left-hand and right-hand
        /// operands.
        /// </summary>
        /// <param name="lhs">The left-hand operand.</param>
        /// <param name="rhs">The right-hand operand.</param>
        /// <returns>The result of the operation.</returns>
        double Apply(double lhs, double rhs);
    }
}
