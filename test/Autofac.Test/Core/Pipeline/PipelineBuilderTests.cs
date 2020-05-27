using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Core;
using Autofac.Core.Diagnostics;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving;
using Autofac.Core.Resolving.Middleware;
using Autofac.Core.Resolving.Pipeline;
using Xunit;

namespace Autofac.Test.Core.Pipeline
{
    public class PipelineBuilderTests
    {
        [Fact]
        public void CanHaveSingleStage()
        {
            var pipelineBuilder = new ResolvePipelineBuilder();
            var order = new List<string>();
            pipelineBuilder.Use(PipelinePhase.RequestStart, (ctxt, next) =>
            {
                order.Add("1");
                next(ctxt);
            });

            var built = pipelineBuilder.Build();

            built.Invoke(new MockPipelineRequestContext());

            Assert.Collection(
                order,
                e => Assert.Equal("1", e));
        }

        [Fact]
        public void CanAddMiddlewareInPhaseOrder()
        {
            var pipelineBuilder = new ResolvePipelineBuilder();
            var order = new List<string>();
            pipelineBuilder.Use("1", PipelinePhase.RequestStart, (ctxt, next) =>
            {
                order.Add("1");
                next(ctxt);
            });
            pipelineBuilder.Use("2", PipelinePhase.ScopeSelection, (ctxt, next) =>
            {
                order.Add("2");
                next(ctxt);
            });
            pipelineBuilder.Use("3", PipelinePhase.Activation, (ctxt, next) =>
            {
                order.Add("3");
                next(ctxt);
            });

            var built = pipelineBuilder.Build();

            built.Invoke(new MockPipelineRequestContext());

            Assert.Collection(
                order,
                el => Assert.Equal("1", el.ToString()),
                el => Assert.Equal("2", el.ToString()),
                el => Assert.Equal("3", el.ToString()));
        }

        [Fact]
        public void CanAddMiddlewareInReversePhaseOrder()
        {
            var pipelineBuilder = new ResolvePipelineBuilder();

            var order = new List<string>();
            pipelineBuilder.Use("3", PipelinePhase.Activation, (ctxt, next) =>
            {
                order.Add("3");
                next(ctxt);
            });
            pipelineBuilder.Use("2", PipelinePhase.ScopeSelection, (ctxt, next) =>
            {
                order.Add("2");
                next(ctxt);
            });
            pipelineBuilder.Use("1", PipelinePhase.RequestStart, (ctxt, next) =>
            {
                order.Add("1");
                next(ctxt);
            });

            var built = pipelineBuilder.Build();

            built.Invoke(new MockPipelineRequestContext());

            Assert.Collection(
                order,
                el => Assert.Equal("1", el.ToString()),
                el => Assert.Equal("2", el.ToString()),
                el => Assert.Equal("3", el.ToString()));
        }

        [Fact]
        public void CanAddMiddlewareInMixedPhaseOrder()
        {
            var pipelineBuilder = new ResolvePipelineBuilder();

            var order = new List<string>();
            pipelineBuilder.Use("3", PipelinePhase.ParameterSelection, (ctxt, next) =>
            {
                order.Add("3");
                next(ctxt);
            });
            pipelineBuilder.Use("4", PipelinePhase.Activation, (ctxt, next) =>
            {
                order.Add("4");
                next(ctxt);
            });
            pipelineBuilder.Use("1", PipelinePhase.RequestStart, (ctxt, next) =>
            {
                order.Add("1");
                next(ctxt);
            });
            pipelineBuilder.Use("2", PipelinePhase.ScopeSelection, (ctxt, next) =>
            {
                order.Add("2");
                next(ctxt);
            });

            var built = pipelineBuilder.Build();

            built.Invoke(new MockPipelineRequestContext());

            Assert.Collection(
                order,
                el => Assert.Equal("1", el.ToString()),
                el => Assert.Equal("2", el.ToString()),
                el => Assert.Equal("3", el.ToString()),
                el => Assert.Equal("4", el.ToString()));
        }

        [Fact]
        public void CanControlPhaseAddPriority()
        {
            var pipelineBuilder = new ResolvePipelineBuilder();
            var order = new List<string>();
            pipelineBuilder.Use("2", PipelinePhase.Activation, (ctxt, next) =>
            {
                order.Add("2");
                next(ctxt);
            });
            pipelineBuilder.Use("1", PipelinePhase.Activation, MiddlewareInsertionMode.StartOfPhase, (ctxt, next) =>
            {
                order.Add("1");
                next(ctxt);
            });
            pipelineBuilder.Use("3", PipelinePhase.Activation, (ctxt, next) =>
            {
                order.Add("3");
                next(ctxt);
            });

            var built = pipelineBuilder.Build();

            built.Invoke(new MockPipelineRequestContext());

            Assert.Collection(
                order,
                el => Assert.Equal("1", el.ToString()),
                el => Assert.Equal("2", el.ToString()),
                el => Assert.Equal("3", el.ToString()));
        }

        [Fact]
        public void CanControlPhaseAddPriorityWithPrecedingPhase()
        {
            var pipelineBuilder = new ResolvePipelineBuilder();

            var order = new List<string>();
            pipelineBuilder.Use("3", PipelinePhase.ScopeSelection, (ctxt, next) =>
            {
                order.Add("3");
                next(ctxt);
            });
            pipelineBuilder.Use("1", PipelinePhase.RequestStart, (ctxt, next) =>
            {
                order.Add("1");
                next(ctxt);
            });
            pipelineBuilder.Use("2", PipelinePhase.ScopeSelection, MiddlewareInsertionMode.StartOfPhase, (ctxt, next) =>
            {
                order.Add("2");
                next(ctxt);
            });

            var built = pipelineBuilder.Build();

            built.Invoke(new MockPipelineRequestContext());

            Assert.Collection(
                order,
                el => Assert.Equal("1", el.ToString()),
                el => Assert.Equal("2", el.ToString()),
                el => Assert.Equal("3", el.ToString()));
        }

        [Fact]
        public void CanAddMultipleMiddlewareToEmptyPipeline()
        {
            var pipelineBuilder = new ResolvePipelineBuilder();
            var order = new List<string>();

            pipelineBuilder.UseRange(new[]
            {
                new DelegateMiddleware("1", PipelinePhase.RequestStart, (ctxt, next) =>
                {
                    order.Add("1");
                    next(ctxt);
                }),
                new DelegateMiddleware("2", PipelinePhase.ScopeSelection, (ctxt, next) =>
                {
                    order.Add("2");
                    next(ctxt);
                }),
                new DelegateMiddleware("4", PipelinePhase.Activation, (ctxt, next) =>
                {
                    order.Add("3");
                    next(ctxt);
                })
            });

            var built = pipelineBuilder.Build();

            built.Invoke(new MockPipelineRequestContext());

            Assert.Collection(
                order,
                el => Assert.Equal("1", el.ToString()),
                el => Assert.Equal("2", el.ToString()),
                el => Assert.Equal("3", el.ToString()));
        }

        [Fact]
        public void AddMultipleMiddlewareOutOfOrderThrows()
        {
            var pipelineBuilder = new ResolvePipelineBuilder();
            var order = new List<string>();

            pipelineBuilder.UseRange(new[]
            {
                new DelegateMiddleware("1", PipelinePhase.RequestStart, (ctxt, next) =>
                {
                    order.Add("1");
                    next(ctxt);
                }),
                new DelegateMiddleware("2", PipelinePhase.ScopeSelection, (ctxt, next) =>
                {
                    order.Add("2");
                    next(ctxt);
                }),
                new DelegateMiddleware("4", PipelinePhase.Activation, (ctxt, next) =>
                {
                    order.Add("3");
                    next(ctxt);
                })
            });

            Assert.Throws<InvalidOperationException>(() => pipelineBuilder.UseRange(new[]
            {
                new DelegateMiddleware("1", PipelinePhase.RequestStart, (ctxt, next) =>
                {
                    order.Add("1");
                    next(ctxt);
                }),
                new DelegateMiddleware("4", PipelinePhase.Activation, (ctxt, next) =>
                {
                    order.Add("4");
                    next(ctxt);
                }),
                new DelegateMiddleware("2", PipelinePhase.ScopeSelection, (ctxt, next) =>
                {
                    order.Add("2");
                    next(ctxt);
                }),
            }));
        }

        [Fact]
        public void AddMultipleMiddlewareToPopulatedPipelineOutOfOrderThrows()
        {
            var pipelineBuilder = new ResolvePipelineBuilder();
            var order = new List<string>();

            Assert.Throws<InvalidOperationException>(() => pipelineBuilder.UseRange(new[]
            {
                new DelegateMiddleware("1", PipelinePhase.RequestStart, (ctxt, next) =>
                {
                    order.Add("1");
                    next(ctxt);
                }),
                new DelegateMiddleware("4", PipelinePhase.Activation, (ctxt, next) =>
                {
                    order.Add("4");
                    next(ctxt);
                }),
                new DelegateMiddleware("2", PipelinePhase.ScopeSelection, (ctxt, next) =>
                {
                    order.Add("2");
                    next(ctxt);
                }),
            }));
        }

        [Fact]
        public void CanAddMultipleMiddlewareToPipelineWithExistingMiddleware()
        {
            var pipelineBuilder = new ResolvePipelineBuilder();
            var order = new List<string>();

            pipelineBuilder.UseRange(new[]
            {
                new DelegateMiddleware("1", PipelinePhase.RequestStart, (ctxt, next) =>
                {
                    order.Add("1");
                    next(ctxt);
                }),
                new DelegateMiddleware("3", PipelinePhase.ScopeSelection, (ctxt, next) =>
                {
                    order.Add("3");
                    next(ctxt);
                }),
                new DelegateMiddleware("5", PipelinePhase.Activation, (ctxt, next) =>
                {
                    order.Add("5");
                    next(ctxt);
                })
            });

            pipelineBuilder.UseRange(new[]
            {
                new DelegateMiddleware("2", PipelinePhase.RequestStart, (ctxt, next) =>
                {
                    order.Add("2");
                    next(ctxt);
                }),
                new DelegateMiddleware("4", PipelinePhase.ScopeSelection, (ctxt, next) =>
                {
                    order.Add("4");
                    next(ctxt);
                }),
                new DelegateMiddleware("6", PipelinePhase.Activation, (ctxt, next) =>
                {
                    order.Add("6");
                    next(ctxt);
                })
            });

            var built = pipelineBuilder.Build();

            built.Invoke(new MockPipelineRequestContext());

            Assert.Collection(
                order,
                el => Assert.Equal("1", el.ToString()),
                el => Assert.Equal("2", el.ToString()),
                el => Assert.Equal("3", el.ToString()),
                el => Assert.Equal("4", el.ToString()),
                el => Assert.Equal("5", el.ToString()),
                el => Assert.Equal("6", el.ToString()));
        }

        private class MockPipelineRequestContext : ResolveRequestContextBase
        {
            public MockPipelineRequestContext()
                : base(
                      new ResolveOperation(new MockLifetimeScope()),
                      new ResolveRequest(new TypedService(typeof(int)), Mocks.GetComponentRegistration(), Enumerable.Empty<Parameter>()),
                      new MockLifetimeScope(),
                      null)
            {
            }

            public override object ResolveComponent(ResolveRequest request)
            {
                throw new NotImplementedException();
            }

            public override object ResolveComponentWithNewOperation(ResolveRequest request)
            {
                throw new NotImplementedException();
            }
        }

        private class MockLifetimeScope : ISharingLifetimeScope
        {
            public ISharingLifetimeScope RootLifetimeScope => throw new NotImplementedException();

            public ISharingLifetimeScope ParentLifetimeScope => throw new NotImplementedException();

            public IDisposer Disposer => throw new NotImplementedException();

            public object Tag => throw new NotImplementedException();

            public IComponentRegistry ComponentRegistry => throw new NotImplementedException();

            public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning
            {
                add { }
                remove { }
            }

            public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding
            {
                add { }
                remove { }
            }

            public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning
            {
                add { }
                remove { }
            }

            public void AttachTrace(IResolvePipelineTracer tracer)
            {
                throw new NotImplementedException();
            }

            public ILifetimeScope BeginLifetimeScope()
            {
                throw new NotImplementedException();
            }

            public ILifetimeScope BeginLifetimeScope(object tag)
            {
                throw new NotImplementedException();
            }

            public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction)
            {
                throw new NotImplementedException();
            }

            public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction)
            {
                throw new NotImplementedException();
            }

            public object CreateSharedInstance(Guid id, Func<object> creator)
            {
                throw new NotImplementedException();
            }

            public object CreateSharedInstance(Guid primaryId, Guid? qualifyingId, Func<object> creator)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public ValueTask DisposeAsync()
            {
                throw new NotImplementedException();
            }

            public object ResolveComponent(ResolveRequest request)
            {
                throw new NotImplementedException();
            }

            public bool TryGetSharedInstance(Guid id, out object value)
            {
                throw new NotImplementedException();
            }

            public bool TryGetSharedInstance(Guid primaryId, Guid? qualifyingId, out object value)
            {
                throw new NotImplementedException();
            }
        }
    }
}
