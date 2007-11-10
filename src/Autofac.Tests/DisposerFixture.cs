using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Autofac.Tests.Component.Ownership
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
			var target = new Disposer();
			var instance = new DisposeTracker();
            target.AddInstanceForDisposal(instance);
			Assert.IsFalse(instance.IsDisposed);
			var refer = new WeakReference(instance);
			Assert.IsTrue(refer.IsAlive);
			instance = null;
			GC.Collect();
			Assert.IsFalse(refer.IsAlive);
			target.Dispose();
		}
	}
}
