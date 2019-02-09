using Xunit;
using StructureMap;
using NSubstitute;

namespace StructureMapTests
{
    public class Tests
    {
        [Fact]
        public void Test1()
        {
            using (var container = new Container(cfg => cfg.IncludeRegistry<FooRegistry>()))
            {
                _ = container.GetInstance<IBarFactory>();
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
    }
}
