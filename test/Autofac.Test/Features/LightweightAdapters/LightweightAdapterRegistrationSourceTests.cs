// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.LightweightAdapters;

namespace Autofac.Test.Features.LightweightAdapters;

[SuppressMessage("CA1052", "CA1052", Justification = "Test containers may not be static.")]
public class LightweightAdapterRegistrationSourceTests
{
    [SuppressMessage("CA1034", "CA1034", Justification = "Type is used as a test scenario/context holder.")]
    public class AdaptingFromOneServiceToAnother
    {
        private readonly Service _from = new TypedService(typeof(object));

        private readonly Service _to = new KeyedService("name", typeof(object));

        private readonly IEnumerable<IComponentRegistration> _adaptedFrom = new[]
        {
            RegistrationBuilder.ForType<object>().CreateRegistration(),
            RegistrationBuilder.ForType<object>().CreateRegistration(),
        };

        private readonly LightweightAdapterRegistrationSource _subject;

        private readonly IEnumerable<IComponentRegistration> _adaptedTo;

        public AdaptingFromOneServiceToAnother()
        {
            var ad = new LightweightAdapterActivatorData(_from, (c, p, t) => new object());
            var rd = new RegistrationData(_to);
            _subject = new LightweightAdapterRegistrationSource(rd, ad);

            _adaptedTo = _subject.RegistrationsFor(_to, s => _adaptedFrom.Select(x => Mocks.GetResolvableImplementation(x)));
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

    [SuppressMessage("CA1034", "CA1034", Justification = "Type is used as a test scenario/context holder.")]
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
