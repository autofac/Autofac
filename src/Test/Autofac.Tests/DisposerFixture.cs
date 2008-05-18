using System;
using NUnit.Framework;

namespace Autofac.Tests
{
	[TestFixture]
	public class DisposerFixture
	{
		[Test]
		public void InstanceDisposed()
		{
			var target = new Disposer();
			var instance = new DisposeTracker();
			target.AddInstanceForDisposal(instance);
			Assert.IsFalse(instance.IsDisposed);
			target.Dispose();
			Assert.IsTrue(instance.IsDisposed);
		}

		[Test]
		public void ReferenceNotHeld()
		{
            DisposeTracker instance;
            WeakReference refer;
            CreateInstanceAndWeakRefInNewStackFrameForMono(out instance, out refer);
			Assert.IsTrue(refer.IsAlive);
			instance = null;
            GC.Collect();
			Assert.IsFalse(refer.IsAlive);
		}

        private static void CreateInstanceAndWeakRefInNewStackFrameForMono(out DisposeTracker instance, out WeakReference refer)
        {
            var target = new Disposer();
            instance = new DisposeTracker();
            target.AddInstanceForDisposal(instance);
            Assert.IsFalse(instance.IsDisposed);
            refer = new WeakReference(instance);
        }
	}
}
