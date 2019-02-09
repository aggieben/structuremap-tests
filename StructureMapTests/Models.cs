using StructureMap;

namespace StructureMapTests
{
    public interface IBarFactory { }

    public interface IFooProvider
    {
        IFoo CreateFoo();
        IFooRepository Repository { get; }
    }

    public interface IFooRepository
    {
        IFoo GetFoo();
    }

    public interface IBazService { }

    public interface IFoo { }
    public class Foo : IFoo { }

    public class BarFactory : IBarFactory
    {
        private readonly IFooProvider _fooProvider;

        public BarFactory(IFooProvider fooProvider)
        {
            _fooProvider = fooProvider;
        }
    }

    public class FooProvider : IFooProvider
    {
        public FooProvider(IFooRepository fooRepository)
        {
            Repository = fooRepository;
        }

        public IFoo CreateFoo()
        {
            return Repository.GetFoo();
        }

        public IFooRepository Repository { get; }
    }

    public class FooRepository : IFooRepository
    {

        public FooRepository()
        {
        }

        public IFoo GetFoo() => new Foo();
    }

    public class CachedFooRepository : IFooRepository
    {
        private readonly IFoo _foo = new Foo();

        public CachedFooRepository(IFooRepository inner, IBazService bazSvc)
        {
            Inner = inner;
        }

        public IFooRepository Inner { get; }
        public IFoo GetFoo() => _foo;
    }
}
