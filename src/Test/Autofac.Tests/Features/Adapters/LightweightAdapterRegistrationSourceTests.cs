using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.LightweightAdapters;
using NUnit.Framework;

namespace Autofac.Tests.Features.Adapters
{
    public class LightweightAdapterRegistrationSourceTests
    {
        [TestFixture]
        public class AdaptingFromOneServiceToAnother
        {
            readonly Service _from = new TypedService(typeof(object)),
                    _to = new NamedService("name", typeof(object));

            readonly IEnumerable<IComponentRegistration> _adaptedFrom = new[]
                {
                    RegistrationBuilder.ForType<object>().CreateRegistration(),
                    RegistrationBuilder.ForType<object>().CreateRegistration()
                };

            readonly LightweightAdapterRegistrationSource _subject;

            readonly IEnumerable<IComponentRegistration> _adaptedTo;

            public AdaptingFromOneServiceToAnother()
            {
                var ad = new LightweightAdapterActivatorData(_from, (c, p, t) => new object());
                var rd = new RegistrationData(_to);
                _subject = new LightweightAdapterRegistrationSource(rd, ad);

                _adaptedTo = _subject.RegistrationsFor(_to, s => _adaptedFrom);
            }

            [Test]
            public void ForEachAdaptedServiceAnAdapterIsReturned()
            {
                Assert.AreEqual(_adaptedFrom.Count(), _adaptedTo.Count());
            }

            [Test]
            public void TheAdaptersExposeTheToService()
            {
                Assert.That(_adaptedTo.All(a => a.Services.Contains(_to)));
            }

            [Test]
            public void TheAdaptersTargetTheSourceServices()
            {
                Assert.That(_adaptedFrom.All(from => _adaptedTo.Any(to => to.Target == from)));
            }
        }

        [TestFixture]
        public class ConstructingAnAdapterRegistrationSource
        {
            [Test]
            public void FromAndToMustDiffer()
            {
                var ad = new LightweightAdapterActivatorData(new TypedService(typeof(object)), (c, p, t) => new object());
                var rd = new RegistrationData(new TypedService(typeof(object)));

                Assert.Throws<ArgumentException>(() =>
                    new LightweightAdapterRegistrationSource(rd, ad));
            }
        }
    }
}
