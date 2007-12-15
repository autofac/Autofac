using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Calculator.Api;

namespace Calculator
{
    class Calculator
    {
        IDictionary<string, IOperation> _operations = new Dictionary<string, IOperation>();

        public Calculator(IEnumerable<IOperation> operations)
        {
            if (operations == null)
                throw new ArgumentNullException("operations");

            foreach (IOperation op in operations)
                _operations.Add(op.Operator, op);
        }

        public IEnumerable<string> AvailableOperators
        {
            get
            {
                return _operations.Keys;
            }
        }

        public double ApplyOperator(string op, double lhs, double rhs)
        {
            if (op == null)
                throw new ArgumentNullException("op");

            IOperation operation;
            if (!_operations.TryGetValue(op, out operation))
                throw new ArgumentException("Unsupported operation.");

            return operation.Apply(lhs, rhs);
        }
    }
}
