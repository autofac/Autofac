using Autofac;
using Autofac.Core.Registration;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Represent Semaphore scoping extensions.
    /// </summary>
    public static class AsyncExtensions
    {
        public static ContainerBuilder WithTimeout(
            this ContainerBuilder container,
            TimeSpan timeout)
        {
            if (container.Properties.ContainsKey(AsyncLock.LockTimeoutKey))
            {
                container.Properties[AsyncLock.LockTimeoutKey] = timeout;
            }
            else
            {
                container.Properties.Add(AsyncLock.LockTimeoutKey, timeout);
            }

            return container;
        }
    }
}
