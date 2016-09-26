using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using Caelan.DynamicLinq.Classes;
using Caelan.DynamicLinq.Extensions;
using ClassBuilder.Classes;
using ClassBuilder.Interfaces;
using UnitOfWork.NET.Interfaces;
namespace UnitOfWork.NET.Classes
{
    public class Repository : IRepository
    {
        public IUnitOfWork UnitOfWork { get; }
        public Repository(IUnitOfWork manager)
        {
            UnitOfWork = manager;
        }
    }

    public class Repository<T> : Repository, IRepository<T> where T : class, new()
    {
        public Repository(IUnitOfWork manager) : base(manager)
        {
        }

        public IQueryable<T> Data => UnitOfWork.Data<T>();
        public T Element(Func<T, bool> expr) => Data.FirstOrDefault(expr);
        public IQueryable<T> All() => Data;
        public IQueryable<T> All(Func<T, bool> expr) => Data.Where(expr);
        public bool Exists(Func<T, bool> expr) => Data.Any(expr);
        public int Count(Func<T, bool> expr) => Data.Count(expr);
        public async Task<T> ElementAsync(Func<T, bool> expr) => await new TaskFactory().StartNew(() => Element(expr));
    }

    public class Repository<TSource, TDestination> : Repository<TSource>, IRepository<TSource, TDestination> where TSource : class, new() where TDestination : class, new()
    {
        public IMapper<TSource, TDestination> DestinationMapper { get; set; }
        public IMapper<TDestination, TSource> SourceMapper { get; set; }

        public Repository(IUnitOfWork manager) : base(manager)
        {
        }

        public IEnumerable<TDestination> AllBuilt() => Builder.BuildList(All()).ToList<TDestination>();

        public IEnumerable<TDestination> AllBuilt(Func<TSource, bool> expr) => Builder.BuildList(All(expr)).ToList<TDestination>();

        public TDestination ElementBuilt(Func<TSource, bool> expr) => Builder.Build(Element(expr)).To<TDestination>();

        public DataSourceResult<TDestination> DataSource(int take, int skip, ICollection<Sort> sort, Filter filter, Func<TSource, bool> expr) => DataSource(take, skip, sort, filter, expr, t => Builder.BuildList(t).ToList<TDestination>());

        private DataSourceResult<TDestination> DataSource(int take, int skip, ICollection<Sort> sort, Filter filter, Func<TSource, bool> expr, Func<IEnumerable<TSource>, IEnumerable<TDestination>> buildFunc)
        {
            var orderBy = typeof(TSource).GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(t => t.Name).FirstOrDefault();

            var res = All(expr);

            if (orderBy != null)
                res = res.OrderBy(orderBy);

            var ds = res.AsQueryable().ToDataSourceResult(take, skip, sort, filter);


            return new DataSourceResult<TDestination>
            {
                Data = buildFunc(ds.Data).ToList(),
                Total = ds.Total
            };
        }


        public async Task<IEnumerable<TDestination>> AllBuiltAsync() => await new TaskFactory().StartNew(AllBuilt);

        public async Task<IEnumerable<TDestination>> AllBuiltAsync(Func<TSource, bool> expr) => await new TaskFactory().StartNew(() => AllBuilt(expr));

        public async Task<TDestination> ElementBuiltAsync(Func<TSource, bool> expr) => await new TaskFactory().StartNew(() => ElementBuilt(expr));

        public async Task<DataSourceResult<TDestination>> DataSourceAsync(int take, int skip, ICollection<Sort> sort, Filter filter, Func<TSource, bool> expr) => await new TaskFactory().StartNew(() => DataSource(take, skip, sort, filter, expr));
    }

    public class Repository<TSource, TDestination, TListDestination> : Repository<TSource, TDestination>, IListRepository<TSource, TDestination, TListDestination> where TSource : class, new() where TDestination : class, new() where TListDestination : class, new()
    {
        public Repository(IUnitOfWork manager) : base(manager)
        {
            ListRepository = manager.Repository<TSource, TListDestination>();
        }

        public IRepository<TSource, TListDestination> ListRepository { get; }
    }
}

