using Autofac.Core.Resolving.Middleware;

namespace Autofac.Benchmarks;

/// <summary>
/// Tests the performance of retrieving a very deeply-nested object graph.
/// </summary>
public class DeepGraphResolveBenchmark
{
    private IContainer _container;

    [GlobalSetup(Target = nameof(Resolve16Deep))]
    public void Setup()
    {
        SetUpContainer();
    }

    [GlobalSetup(Target = nameof(Resolve16DeepWithCircularDependencyChecksDisabled))]
    public void DisableCircularDependencyChecks()
    {
        DefaultMiddlewareConfiguration.UnsafeDisableProactiveCircularDependencyChecks();
        SetUpContainer();
    }

    private void SetUpContainer()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<A>();
        builder.RegisterType<B>();
        builder.RegisterType<C>();
        builder.RegisterType<D>();
        builder.RegisterType<E>();
        builder.RegisterType<F>();
        builder.RegisterType<G>();
        builder.RegisterType<H>();
        builder.RegisterType<I>();
        builder.RegisterType<J>();
        builder.RegisterType<K>();
        builder.RegisterType<L>();
        builder.RegisterType<M>();
        builder.RegisterType<N>();
        builder.RegisterType<O>();
        builder.RegisterType<P>();
        _container = builder.Build();
    }

    [GlobalCleanup(Target = nameof(Resolve16DeepWithCircularDependencyChecksDisabled))]
    public void EnableCircularDependencyChecks()
    {
        DefaultMiddlewareConfiguration.EnableProactiveCircularDependencyChecks();
    }

    [Benchmark(Baseline = true )]
    public void Resolve16Deep()
    {
        var instance = _container.Resolve<A>();
        GC.KeepAlive(instance);
    }

    [Benchmark]
    public void Resolve16DeepWithCircularDependencyChecksDisabled()
    {
        var instance = _container.Resolve<A>();
        GC.KeepAlive(instance);
    }
    
    internal class A
    {
        public A(B b) { }
    }

    internal class B
    {
        public B(C c) { }
    }
    
    internal class C
    {
        public C(D d) { }
    }
    
    internal class D
    {
        public D(E e) { }
    }

    internal class E
    {
        public E(F f) { }
    }

    internal class F
    {
        public F(G g) { }
    }

    internal class G
    {
        public G(H h) { }
    }

    internal class H
    {
        public H(I i) { }
    }

    internal class I
    {
        public I(J j) { }
    }

    internal class J
    {
        public J(K k) { }
    }

    internal class K
    {
        public K(L l) { }
    }

    internal class L
    {
        public L(M m) { }
    }

    internal class M
    {
        public M(N n) { }
    }

    internal class N
    {
        public N(O o) { }
    }

    internal class O
    {
        public O(P p) { }
    }

    internal class P
    {
    }

}
