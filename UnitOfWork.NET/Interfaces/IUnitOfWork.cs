using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitOfWork.NET.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        IEnumerable<T> Data<T>() where T : class, new();

        /// <summary>
        /// 
        /// </summary>
        TRepository CustomRepository<TRepository>() where TRepository : IRepository;

        /// <summary>
        /// 
        /// </summary>
        IRepository<T> Repository<T>() where T : class, new();

        /// <summary>
        /// 
        /// </summary>
        IRepository<TSource, TDestination> Repository<TSource, TDestination>() where TSource : class, new() where TDestination : class, new();

        /// <summary>
        /// 
        /// </summary>
        IListRepository<TSource, TDestination, TListDestination> Repository<TSource, TDestination, TListDestination>() where TSource : class, new() where TDestination : class, new() where TListDestination : class, new();
    }
}
