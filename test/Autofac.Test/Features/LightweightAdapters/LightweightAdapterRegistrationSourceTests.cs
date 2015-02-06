using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.LightweightAdapters;
using Xunit;

namespace Autofac.Test.Features.LightweightAdapters
{
    public class LightweightAdapterRegistrationSourceTests
    {
        public class AdaptingFromOneServiceToAnother
        {
            readonly Service _from = new TypedService(typeof(object)),
                    _to = new KeyedService("name", typeof(object));

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

            [Fact]
            public void ForEachAdaptedServiceAnAdapterIsReturned()
            {
                Assert.Equal(_adaptedFrom.Count(), _adaptedTo.Count());
            }

            [Fact]
            public void TheAdaptersExposeTheToService()
            {
                Assert.True(_adaptedTo.All(a => a.Services.Contains(_to)));
            }

            [Fact]
            public void TheAdaptersTargetTheSourceServices()
            {
                Assert.True(_adaptedFrom.All(from => _adaptedTo.Any(to => to.Target == from)));
            }
        }
        public class ConstructingAnAdapterRegistrationSource
        {
            [Fact]
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
