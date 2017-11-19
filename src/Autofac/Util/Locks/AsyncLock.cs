namespace System.Threading.Tasks
{
    /// <summary>
    /// Represent Semaphore scoping.
    /// The semaphore scope will enable the usage of semaphore easy like lock.
    /// </summary>
    public sealed class AsyncLock : IDisposable
    {
        public const string LockTimeoutKey = "__LockTimeout";

        private readonly SemaphoreSlim _gate;
        private readonly TimeSpan _defaultTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLock" /> class.
        /// </summary>
        /// <param name="gateLimit">The number of max concurrent requests (beyond this count request will delay until the completion of other request).
        /// </param>
        /// <param name="defaultTimeout">
        /// The default timeout will cause the waiting call to throw exception,
        /// Maximum waiting in order to acquires the lock-scope, beyond this waiting it will throw TimeoutException.
        /// </param>
        public AsyncLock(TimeSpan defaultTimeout, byte gateLimit = 1)
        {
            _gate = new SemaphoreSlim(gateLimit);
            _defaultTimeout = defaultTimeout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLock" /> class.
        /// </summary>
        /// <param name="gate">The gate.</param>
        /// <param name="defaultTimeout">The default timeout will cause the waiting call to throw exception,
        /// Maximum waiting in order to acquires the lock-scope, beyond this waiting it will throw TimeoutException.</param>
        public AsyncLock(SemaphoreSlim gate, TimeSpan defaultTimeout)
        {
            _gate = gate;
            _defaultTimeout = defaultTimeout;
        }

        /// <summary>
        /// Try to acquire async lock,
        /// when failed the LockScope.Acquired will equals false
        /// </summary>
        /// <param name="overrideTimeout">The override timeout.</param>
        /// <returns>lock disposal and acquired indication</returns>
        public async Task<LockScope> TryAcquireAsync(TimeSpan overrideTimeout = default(TimeSpan))
        {
            var timeout = overrideTimeout == default(TimeSpan) ? _defaultTimeout : overrideTimeout;
            bool acquired = await _gate.WaitAsync(timeout).ConfigureAwait(false);
            return new LockScope(_gate, acquired);
        }

        /// <summary>
        /// Try to acquire async lock,
        /// when it will throw TimeoutException
        /// </summary>
        /// <param name="overrideTimeout">The override timeout.</param>
        /// <returns>lock disposal</returns>
        /// <exception cref="TimeoutException">when acquire lock fail</exception>
        public async Task<IDisposable> AcquireAsync(TimeSpan overrideTimeout = default(TimeSpan))
        {
            var scope = await TryAcquireAsync(overrideTimeout).ConfigureAwait(false);
            if (!scope.Acquired)
            {
                scope.Dispose();
                throw new TimeoutException();
            }

            return scope;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _gate?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AsyncLock"/> class.
        /// </summary>
        ~AsyncLock() => Dispose();
    }
}
