using System;
using NHibernate;
using NHibernate.Cfg;
using Autofac;
using Autofac.Integration.Mvc;

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
                .InstancePerHttpRequest();

            builder.Register(c => new TransactionTracker())
                .InstancePerHttpRequest();

            builder.Register(c => c.Resolve<ISessionFactory>().OpenSession())
                .InstancePerHttpRequest()
                .OnActivated(e => e.Context.Resolve<TransactionTracker>().CurrentTransaction =
                    e.Instance.BeginTransaction());

            builder.Register(c => new Configuration().Configure().BuildSessionFactory())
                .SingleInstance();
        }
    }
}
