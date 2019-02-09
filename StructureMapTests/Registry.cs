using StructureMap;
using NSubstitute;

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
}
