using StructureMap;
using NSubstitute;
using System.Collections.Generic;
using System.Collections;

namespace StructureMapTests
{
    public class SingletonRegistry : Registry
    {
        public SingletonRegistry()
        {
            For<IFoo>().Singleton().Use<Foo>();
            For<IBar>().Singleton().Use<Bar>();
            For<IBazService>().Singleton().Use<BazService>();
            For<IBarFactory>().Singleton().Use<BarFactory>();
            For<IFooProvider>().Singleton().Use<FooProvider>();
            For<IFooRepository>().Singleton().Use<FooRepository>()
                .DecorateWith((ctx, inner) => new CachedFooRepository(inner, ctx.GetInstance<IBazService>()));
        }
    }

    public class TransientRegistry : Registry
    {
        public TransientRegistry()
        {
            For<IFoo>().Singleton().Use<Foo>();
            For<IBar>().Singleton().Use<Bar>();
            For<IBazService>().Singleton().Use<BazService>();
            For<IBarFactory>().Singleton().Use<BarFactory>();
            For<IFooProvider>().Singleton().Use<FooProvider>();
            For<IFooRepository>().Singleton().Use<FooRepository>()
                .DecorateWith((ctx, inner) => new CachedFooRepository(inner, ctx.GetInstance<IBazService>()));
        }
    }

    public class FooBarTransientRegistry : Registry
    {
        public FooBarTransientRegistry()
        {
            For<IFoo>().Use<Foo>();
            For<IBar>().Use<Bar>();
            For<IFooRepository>().Use<FooRepository>();
        }
    }

    public class FooProviderTransientRegistry : FooBarTransientRegistry
    {
        public FooProviderTransientRegistry() : base()
        {
            For<IFooProvider>().Use<FooProvider>();
        }
    }

    public class BarFactoryTransientRegistry : FooProviderTransientRegistry
    {
        public BarFactoryTransientRegistry() : base()
        {
            For<IBarFactory>().Use<BarFactory>();
        }
    }

    public class FooBarBazTransientRegistry : BarFactoryTransientRegistry
    {
        public FooBarBazTransientRegistry() : base()
        {
            For<IBazService>().Use<BazService>();
        }
    }

    public class NoDecorationsRegistry : Registry
    {
        public NoDecorationsRegistry() : base()
        {
            For<IFoo>().Singleton().Use<Foo>();
            For<IBar>().Singleton().Use<Bar>();
            For<IBazService>().Singleton().Use<BazService>();
            For<IBarFactory>().Singleton().Use<BarFactory>();
            For<IFooProvider>().Singleton().Use<FooProvider>();
            For<IFooRepository>().Singleton().Use<FooRepository>();
        }
    }

    public class SimpleSingletonRegistry : Registry
    {
        public SimpleSingletonRegistry()
        {
            For<IFoo>().Singleton().Use<Foo>();
            For<IBar>().Singleton().Use<Bar>();
            For<IFooRepository>().Singleton().Use<FooRepository>();
        }
    }

    public class FooProviderSingletonRegistry : SimpleSingletonRegistry
    {
        public FooProviderSingletonRegistry() : base()
        {
            For<IFooProvider>().Singleton().Use<FooProvider>();
        }
    }

    public class BarFactorySingletonRegistry : FooProviderSingletonRegistry
    {
        public BarFactorySingletonRegistry() : base()
        {
            For<IBarFactory>().Singleton().Use<BarFactory>();
        }
    }

    public class BazServiceSingletonRegistry : BarFactorySingletonRegistry
    {
        public BazServiceSingletonRegistry() : base()
        {
            For<IBazService>().Singleton().Use<BazService>();
        }
    }

    public class RegistryClassData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { new FooProviderTransientRegistry() };
            yield return new object[] { new BarFactoryTransientRegistry() };
            yield return new object[] { new FooBarBazTransientRegistry() };
            yield return new object[] { new FooProviderSingletonRegistry() };
            yield return new object[] { new BarFactorySingletonRegistry() };
            yield return new object[] { new BazServiceSingletonRegistry() };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
