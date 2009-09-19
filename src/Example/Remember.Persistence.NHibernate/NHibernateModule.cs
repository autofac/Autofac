using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Builder;
using NHibernate;
using System.Data;
using NHibernate.Cfg;
using Remember.Model;
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
                .ShareInstanceIn(WebLifetime.Request);

            builder.RegisterDelegate(c => new TransactionTracker())
                .ShareInstanceIn(WebLifetime.Request);

            builder.RegisterDelegate(c => c.Resolve<ISessionFactory>().OpenSession())
                .ShareInstanceIn(WebLifetime.Request)
                .OnActivated(e =>
                {
                    e.Context.Resolve<TransactionTracker>().CurrentTransaction = ((ISession)e.Instance).BeginTransaction();
                });

            builder.RegisterDelegate(c => new Configuration().Configure().BuildSessionFactory())
                .SingleSharedInstance();
        }
    }
}
