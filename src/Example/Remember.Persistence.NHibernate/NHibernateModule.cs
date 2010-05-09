using System;
using NHibernate;
using NHibernate.Cfg;
using Autofac;
using Autofac.Integration.Web;

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
                .InstancePerMatchingLifetimeScope(WebLifetime.Request);

            builder.Register(c => new TransactionTracker())
                .InstancePerMatchingLifetimeScope(WebLifetime.Request);

            builder.Register(c => c.Resolve<ISessionFactory>().OpenSession())
                .InstancePerMatchingLifetimeScope(WebLifetime.Request)
                .OnActivated(e => e.Context.Resolve<TransactionTracker>().CurrentTransaction =
                    e.Instance.BeginTransaction());

            builder.Register(c => new Configuration().Configure().BuildSessionFactory())
                .SingleInstance();
        }
    }
}
