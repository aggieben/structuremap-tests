using Xunit;
using StructureMap;
using NSubstitute;
using System.Collections.Generic;
using FluentAssertions;
using System.IO;
using Xunit.Abstractions;

namespace StructureMapTests
{
    public class Tests
    {
        private readonly ITestOutputHelper _output;
        public Tests(ITestOutputHelper output)
        {
            _output = output;
        }

        public static IEnumerable<object[]> GetRegistries()
        {
            yield return new object[] { new SingletonRegistry() };
            yield return new object[] { new TransientRegistry() };
        }

        [Theory]
        [MemberData(nameof(GetRegistries))]
        public void TransitiveDependencyWithDecoration(Registry registry)
        {
            using (var container = new Container(cfg => cfg.IncludeRegistry(registry)))
            {
                var fooProvider = container.GetInstance<IFooProvider>();
                var cachedRepo = fooProvider.Repository;

                Assert.IsType<CachedFooRepository>(cachedRepo);
                Assert.IsType<FooRepository>(((CachedFooRepository)cachedRepo).Inner);

                var child = container.CreateChildContainer();
                child.Model.For<IFooProvider>().Default.EjectObject();
                child.Model.For<IFooRepository>().Default.EjectObject();

                var fakeFoo = Substitute.For<IFooRepository>(); // new FakeFoo();
                child.Inject(fakeFoo);

                var fooProvider2 = child.GetInstance<IFooProvider>();
                Assert.NotSame(fooProvider, fooProvider2);

                var cachedRepo2 = fooProvider2.Repository;
                Assert.NotSame(cachedRepo, cachedRepo2);

                var innerRepo2 = ((CachedFooRepository)cachedRepo2).Inner;
                Assert.IsType<CachedFooRepository>(cachedRepo2);
                Assert.IsNotType<FooRepository>(innerRepo2); // this should be the mock at this point
            }
        }

        [Theory]
        [MemberData(nameof(GetRegistries))]
        public void TransitiveDependency_BothEjectedInChild(Registry registry)
        {
            using (var container = new Container(cfg => cfg.IncludeRegistry(registry)))
            {
                var bar = container.GetInstance<IBar>();
                Assert.IsType<Foo>(bar.Foo);

                var child = container.CreateChildContainer();
                child.Model.For<IFoo>().Default.EjectAndRemove();
                child.Model.For<IBar>().Default.EjectAndRemove();

                var fakeBar = Substitute.For<IFoo>();
                child.Inject(fakeBar);
                child.Configure(c => c.For<IBar>().Use<Bar>());

                var bar2 = child.GetInstance<IBar>();
                Assert.IsNotType<Foo>(bar2.Foo);
            }
        }

        [Theory]
        [MemberData(nameof(GetRegistries))]
        public void TransitiveDependency_BothClearedInChild(Registry registry)
        {
            using (var container = new Container(cfg => cfg.IncludeRegistry(registry)))
            {
                var bar = container.GetInstance<IBar>();
                Assert.IsType<Foo>(bar.Foo);

                var child = container.CreateChildContainer();
                child.Configure(c => c.For<IFoo>().ClearAll());
                child.Configure(c => c.For<IBar>().ClearAll());

                var fakeBar = Substitute.For<IFoo>();
                child.Inject(fakeBar);
                child.Configure(c => c.For<IBar>().Use<Bar>());

                var bar2 = child.GetInstance<IBar>();
                Assert.IsNotType<Foo>(bar2.Foo);
            }
        }

        [Theory]
        [MemberData(nameof(GetRegistries))]
        public void TransitiveDependencyAfterParentEjection(Registry registry)
        {
            using (var container = new Container(cfg => cfg.IncludeRegistry(registry)))
            {
                var bar = container.GetInstance<IBar>();
                Assert.IsType<Foo>(bar.Foo);

                var child = container.CreateChildContainer();
                container.Configure(c => c.For<IFoo>().ClearAll());
                var fakeBar = Substitute.For<IFoo>();
                child.Inject(fakeBar);

                var bar2 = child.GetInstance<IBar>();
                Assert.IsNotType<Foo>(bar2.Foo);
            }
        }

        [Theory]
        [MemberData(nameof(GetRegistries))]
        public void TransitiveDependencyAfterChildEjection(Registry registry)
        {
            using (var container = new Container(cfg => cfg.IncludeRegistry(registry)))
            {
                var bar = container.GetInstance<IBar>();
                Assert.IsType<Foo>(bar.Foo);

                var fakeBar = Substitute.For<IFoo>();
                var child = container.CreateChildContainer();
                child.Inject(fakeBar);

                var bar2 = child.GetInstance<IBar>();
                Assert.IsNotType<Foo>(bar2.Foo);
            }
        }

        [Theory]
        [MemberData(nameof(GetRegistries))]
        public void TransitiveDependency_BothInjected(Registry registry)
        {
            using (var container = new Container(cfg => cfg.IncludeRegistry(registry)))
            {
                var bar = container.GetInstance<IBar>();
                Assert.IsType<Foo>(bar.Foo);

                var fakeFoo = Substitute.For<IFoo>();
                var child = container.CreateChildContainer();
                child.Configure(cfg =>
                {
                    cfg.For<IFoo>().Use(fakeFoo);
                    cfg.For<IBar>().Use<Bar>();
                });

                var foo2 = child.GetInstance<IFoo>();
                Assert.IsNotType<Foo>(foo2);

                var bar2 = child.GetInstance<IBar>();
                Assert.IsNotType<Foo>(bar2.Foo);
            }
        }

        [Theory]
        [MemberData(nameof(GetRegistries))]
        public void TransitiveDependency(Registry registry)
        {
            using (var container = new Container(cfg => cfg.IncludeRegistry(registry)))
            {
                var bar = container.GetInstance<IBar>();
                Assert.IsType<Foo>(bar.Foo);

                var fakeBar = Substitute.For<IFoo>();
                var child = container.CreateChildContainer();
                child.Inject(fakeBar);

                var bar2 = child.GetInstance<IBar>();
                Assert.IsNotType<Foo>(bar2.Foo);
            }
        }

        [Theory]
        [MemberData(nameof(GetRegistries))]
        public void DirectDependency(Registry registry)
        {
            using (var container = new Container(cfg => cfg.IncludeRegistry(registry)))
            {
                var baz1 = container.GetInstance<IBazService>();
                Assert.IsType<BazService>(baz1);

                var child = container.CreateChildContainer();
                var fakeBaz = Substitute.For<IBazService>();
                child.Inject(fakeBaz);

                var baz2 = child.GetInstance<IBazService>();
                Assert.IsNotType<BazService>(baz2);
            }
        }

        [Fact]
        public void TransitiveDependency_DirectConfiguration()
        {
            var parent = new Container(_ =>
            {
                _.For<IFoo>().Use<Foo>();
                _.For<IBar>().Use<Bar>();
            });

            var child = parent.CreateChildContainer();
            child.Configure(x =>
            {
                x.For<IFoo>().Use(Substitute.For<IFoo>());
            });

            parent.GetInstance<IBar>().Foo
                .Should().BeOfType<Foo>();

            child.GetInstance<IBar>().Foo
                .Should().NotBeOfType<Foo>();
        }

        [Theory]
        [ClassData(typeof(RegistryClassData))]
        public void TransitiveDependency_TheoryA(Registry registry)
        {
            var parent = new Container(_ => _.IncludeRegistry(registry));
            var child = parent.CreateChildContainer();

            child.Configure(x =>
            {
                x.For<IFooRepository>().Use(Substitute.For<IFooRepository>());
            });

            parent.GetInstance<IFooProvider>().Repository
                .Should().BeOfType<FooRepository>();

            child.GetInstance<IFooProvider>().Repository
                .Should().NotBeOfType<FooRepository>();
        }

        [Theory]
        [ClassData(typeof(RegistryClassData))]
        public void TransitiveDependency_TheoryB(Registry registry)
        {
            var parent = new Container(_ => _.IncludeRegistry(registry));
            var child = parent.CreateChildContainer();

            child.Configure(x =>
            {
                x.For<IFoo>().Use(Substitute.For<IFoo>());
            });

            parent.GetInstance<IBar>().Foo
                .Should().BeOfType<Foo>();

            child.GetInstance<IBar>().Foo
                .Should().NotBeOfType<Foo>();
        }

        [Fact]
        public void TransitiveDependency_NoDecorationsRegistry()
        {
            var parent = new Container(_ => _.IncludeRegistry(new NoDecorationsRegistry()));
            var child = parent.CreateChildContainer();

            child.Configure(x =>
            {
                x.For<IFoo>().Use(Substitute.For<IFoo>());
            });

            parent.GetInstance<IBar>().Foo
                .Should().BeOfType<Foo>();

            child.GetInstance<IBar>().Foo
                .Should().NotBeOfType<Foo>();
        }
    }
}
