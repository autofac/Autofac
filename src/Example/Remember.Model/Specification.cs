using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Remember.Model
{
    public abstract class Specification<T>
    {
        public bool IsSatisfiedBy(T candidate)
        {
            return SatisfyingElementsFrom(new[] { candidate }).Any();
        }

        public IEnumerable<T> SatisfyingElementsFrom(IEnumerable<T> candidates)
        {
            if (candidates == null)
                throw new ArgumentNullException("candidates");

            return SatisfyingElementsFrom(candidates.AsQueryable());
        }

        public abstract IQueryable<T> SatisfyingElementsFrom(IQueryable<T> candidates);
    }
}
