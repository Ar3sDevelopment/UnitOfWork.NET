﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Caelan.DynamicLinq.Classes;

namespace UnitOfWork.NET.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRepository
    {
        IUnitOfWork UnitOfWork { get; }
    }

    /// <summary>
    /// The DbSet used by Repository to perform operations on database.
    /// </summary>
    public interface IRepository<out T> : IRepository
    {
        IEnumerable<T> Data { get; }

        /// <summary>
        /// Recovers an element using a where, if multiple returned it returns the first.
        /// </summary>
        /// <param name="expr">The condition for retrieving the element</param>
        T Element(Func<T, bool> expr);

        /// <summary>
        /// All entities of the DbSet
        /// </summary>
        IEnumerable<T> All();

        /// <summary>
        /// All entities of the DbSet filtered by the where.
        /// </summary>
        /// <param name="expr">The condition for filtering all entities</param>
        IEnumerable<T> All(Func<T, bool> expr);

        /// <summary>
        /// Check if exists any element satisfying the given condition.
        /// </summary>
        /// <param name="expr">The condition to check if any element verifies it</param>
        bool Exists(Func<T, bool> expr);

        /// <summary>
        /// Counts all entities satisfying the given condition.
        /// </summary>
        /// <param name="expr">The condition for counting all entities that verify it</param>
        int Count(Func<T, bool> expr);
    }

    public interface IRepository<out TSource, TDestination> : IRepository<TSource> where TSource : class where TDestination : class
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        TDestination ElementBuilt(Func<TSource, bool> expr);

        /// <summary>
        ///
        /// </summary>
        IEnumerable<TDestination> AllBuilt();

        /// <summary>
        /// 
        /// </summary>
        IEnumerable<TDestination> AllBuilt(Func<TSource, bool> expr);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="take"></param>
        /// <param name="skip"></param>
        /// <param name="sort"></param>
        /// <param name="filter"></param>
        /// <param name="expr"></param>
        DataSourceResult<TDestination> DataSource(int take, int skip, ICollection<Sort> sort, Filter filter, Func<TSource, bool> expr);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IListRepository<TSource, TDestination, TListDestination> : IRepository<TSource, TDestination> where TSource : class where TDestination : class where TListDestination : class
    {
        /// <summary>
        /// 
        /// </summary>
        IRepository<TSource, TListDestination> ListRepository { get; }
    }
}