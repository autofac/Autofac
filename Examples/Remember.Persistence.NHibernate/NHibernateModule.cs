using System;
using Autofac;
using NHibernate;
using NHibernate.Cfg;

namespace Remember.Persistence.NHibernate
{
    public class NHibernateModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            builder.RegisterGeneric(typeof(NHibernateRepository<>))
                .As(typeof(IRepository<>))
                .InstancePerRequest();

            builder.Register(c => new TransactionTracker())
                .InstancePerRequest();

            builder.Register(c => c.Resolve<ISessionFactory>().OpenSession())
                .InstancePerRequest()
                .OnActivated(e => e.Context.Resolve<TransactionTracker>().CurrentTransaction =
                    e.Instance.BeginTransaction());

            builder.Register(c => new Configuration().Configure().BuildSessionFactory())
                .SingleInstance();
        }
    }
}
