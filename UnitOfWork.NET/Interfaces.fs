namespace UnitOfWork.NET.Interfaces

open Caelan.DynamicLinq.Classes
open System
open System.Collections.Generic
open System.Linq
open System.Linq.Expressions

type IRepository = 
    /// <summary>
    /// 
    /// </summary>
    abstract UnitOfWork : IUnitOfWork

and IRepository<'T when 'T : not struct> = 
    inherit IRepository
    
    /// <summary>
    /// The DbSet used by Repository to perform operations on database.
    /// </summary>
    abstract Data : IEnumerable<'T>
    
    /// <summary>
    /// Recovers an element using a where, if multiple returned it returns the first.
    /// </summary>
    /// <param name="where">The condition for retrieving the element</param>
    abstract Element : where:Func<'T, bool> -> 'T
    
    /// <summary>
    /// All entities of the DbSet
    /// </summary>
    abstract All : unit -> seq<'T>
    
    /// <summary>
    /// All entities of the DbSet filtered by the where.
    /// </summary>
    /// <param name="where">The condition for filtering all entities</param>
    abstract All : where:Func<'T, bool> -> seq<'T>
    
    /// <summary>
    /// Check if exists any element satisfying the given condition.
    /// </summary>
    /// <param name="expr">The condition to check if any element verifies it</param>
    abstract Exists : expr:Func<'T, bool> -> bool
    
    /// <summary>
    /// Counts all entities satisfying the given condition.
    /// </summary>
    /// <param name="expr">The condition for counting all entities that verify it</param>
    abstract Count : expr:Func<'T, bool> -> int

and IRepository<'TSource, 'TDestination when 'TSource : not struct and 'TDestination : not struct> = 
    inherit IRepository<'TSource>
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="where"></param>
    abstract ElementBuilt : where:Func<'TSource, bool> -> 'TDestination
    
    /// <summary>
    ///
    /// </summary>
    abstract AllBuilt : unit -> seq<'TDestination>
    
    /// <summary>
    /// 
    /// </summary>
    abstract AllBuilt : Func<'TSource, bool> -> seq<'TDestination>
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="take"></param>
    /// <param name="skip"></param>
    /// <param name="sort"></param>
    /// <param name="filter"></param>
    /// <param name="where"></param>
    abstract DataSource : take:int * skip:int * sort:ICollection<Sort> * filter:Filter * where:Func<'TSource, bool> -> DataSourceResult<'TDestination>

and IListRepository<'TSource, 'TDestination, 'TListDestination when 'TSource : not struct and 'TDestination : not struct and 'TListDestination : not struct> = 
    inherit IRepository<'TSource, 'TDestination>
    /// <summary>
    /// 
    /// </summary>
    abstract ListRepository : IRepository<'TSource, 'TListDestination> with get, set

and IUnitOfWork = 
    inherit IDisposable
    
    /// <summary>
    /// 
    /// </summary>
    abstract Data<'T when 'T : not struct> : unit -> IEnumerable<'T>
    
    /// <summary>
    /// 
    /// </summary>
    abstract CustomRepository<'TRepository when 'TRepository :> IRepository> : unit -> 'TRepository
    
    /// <summary>
    /// 
    /// </summary>
    abstract Repository<'T when 'T : not struct> : unit -> IRepository<'T>
    
    /// <summary>
    /// 
    /// </summary>
    abstract Repository<'TSource, 'TDestination when 'TSource : not struct and 'TDestination : not struct> : unit -> IRepository<'TSource, 'TDestination>
    
    /// <summary>
    /// 
    /// </summary>
    abstract Repository<'TSource, 'TDestination, 'TListDestination when 'TSource : not struct and 'TDestination : not struct and 'TListDestination : not struct> : unit -> IListRepository<'TSource, 'TDestination, 'TListDestination>