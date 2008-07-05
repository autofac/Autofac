using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Remember.Model;
using NHibernate;
using NHibernate.Linq;

namespace Remember.Persistence.NHibernate
{
    public class NHibernateRepository<T> : IRepository<T>
        where T : class, IIdentifiable
    {
        ISession _session;

        public NHibernateRepository(ISession session)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            _session = session;
        }

        public IQueryable<T> Items
        {
            get { return _session.Linq<T>(); }
        }

        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            _session.Save(item);
        }

        public void Remove(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            _session.Delete(item);
        }

        public T FindById(int id)
        {
            return _session.Load<T>(id);
        }

        public IEnumerable<T> FindBySpecification(params Specification<T>[] specifications)
        {
            if (specifications == null || specifications.Any(s => s == null))
                throw new ArgumentNullException("specifications");

            var items = Items;

            foreach (var specification in specifications)
                items = specification.SatisfiersFrom(items);

            return items;
        }
    }
}
