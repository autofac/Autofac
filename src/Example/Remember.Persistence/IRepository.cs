using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Remember.Model;

namespace Remember.Persistence
{
    public interface IRepository<T>
        where T : class, IIdentifiable
    {
        IQueryable<T> Items { get; }

        void Add(T item);

        void Remove(T item);

        T FindById(int id);

        IEnumerable<T> FindBySpecification(params Specification<T>[] specifications);
    }
}
