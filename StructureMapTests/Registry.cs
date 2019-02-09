using StructureMap;
using NSubstitute;

namespace StructureMapTests
{
    public class FooRegistry : Registry
    {
        public FooRegistry()
        {
            For<IBazService>().Use(Substitute.For<IBazService>());
            For<IBarFactory>().Singleton().Use<BarFactory>();
            For<IFooProvider>().Singleton().Use<FooProvider>();
            For<IFooRepository>().Singleton().Use<FooRepository>()
                .DecorateWith((ctx, inner) => new CachedFooRepository(inner, ctx.GetInstance<IBazService>()));
        }
    }
}
