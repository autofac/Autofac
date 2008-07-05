using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Builder;
using NHibernate;
using System.Data;
using NHibernate.Cfg;
using Remember.Model;

namespace Remember.Persistence.NHibernate
{
    public class NHibernateModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            builder.RegisterGeneric(typeof(NHibernateRepository<>)).As(typeof(IRepository<>))
                .ContainerScoped();

            builder.Register(c => new TransactionTracker())
                .ContainerScoped();

            builder.Register(c => c.Resolve<ISessionFactory>().OpenSession())
                .ContainerScoped()
                .OnActivated((sender, e) =>
                {
                    e.Context.Resolve<TransactionTracker>().CurrentTransaction = ((ISession)e.Instance).BeginTransaction();
                });

            builder.Register(c => new Configuration().Configure().BuildSessionFactory());
        }
    }
}
