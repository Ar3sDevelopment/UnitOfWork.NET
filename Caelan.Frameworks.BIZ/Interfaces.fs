namespace Caelan.Frameworks.BIZ.Interfaces

open System
open System.Data.Entity
open System.Data.Entity.Infrastructure
open System.Linq
open System.Linq.Expressions
open System.Collections.Generic
open Caelan.DynamicLinq.Classes

type IRepository = 
    /// <summary>
    /// 
    /// </summary>
    abstract UnitOfWork : IUnitOfWork

and IRepository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null> = 
    inherit IRepository
    
    /// <summary>
    /// The DbSet used by Repository to perform operations on database.
    /// </summary>
    abstract Set : DbSet<'TEntity>
    
    /// <summary>
    /// Recovers the entity by primary keys.
    /// </summary>
    /// <param name="ids">The primary keys values</param>
    abstract SingleEntity : [<ParamArray>] ids:obj [] -> 'TEntity
    
    /// <summary>
    /// Recovers an entity using a where, if multiple returned it returns the first.
    /// </summary>
    /// <param name="where">The condition for retrieving the entity</param>
    abstract SingleEntity : where:Expression<Func<'TEntity, bool>> -> 'TEntity
    
    /// <summary>
    /// All entities of the DbSet
    /// </summary>
    abstract All : unit -> IQueryable<'TEntity>
    
    /// <summary>
    /// All entities of the DbSet filtered by the where.
    /// </summary>
    /// <param name="where">The condition for filtering all entities</param>
    abstract All : where:Expression<Func<'TEntity, bool>> -> IQueryable<'TEntity>
    
    /// <summary>
    /// Check if exists any entity satisfying the given condition.
    /// </summary>
    /// <param name="expr">The condition to check if any entity verifies it</param>
    abstract Exists : expr:Expression<Func<'TEntity, bool>> -> bool
    
    /// <summary>
    /// Counts all entities satisfying the given condition.
    /// </summary>
    /// <param name="expr">The condition for counting all entities that verify it</param>
    abstract Count : expr:Expression<Func<'TEntity, bool>> -> int
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    abstract Insert : entity:'TEntity -> 'TEntity
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="ids"></param>
    abstract Update : entity:'TEntity * [<ParamArray>] ids:obj [] -> unit
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ids"></param>
    abstract Delete : [<ParamArray>] ids:obj [] -> unit
    
    ///<summary>
    ///
    ///</summary>
    /// <param name="entities"></param>
    abstract OnSaveChanges : [<ParamArray>] entities:'TEntity [] -> unit

and IRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct> = 
    inherit IRepository<'TEntity>
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ids"></param>
    abstract SingleDTO : [<ParamArray>] ids:obj [] -> 'TDTO
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="where"></param>
    abstract SingleDTO : where:Expression<Func<'TEntity, bool>> -> 'TDTO
    
    /// <summary>
    ///
    /// </summary>
    abstract List : unit -> seq<'TDTO>
    
    /// <summary>
    /// 
    /// </summary>
    abstract List : Expression<Func<'TEntity, bool>> -> seq<'TDTO>
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="take"></param>
    /// <param name="skip"></param>
    /// <param name="sort"></param>
    /// <param name="filter"></param>
    /// <param name="where"></param>
    abstract All : take:int * skip:int * sort:ICollection<Sort> * filter:Filter * where:Expression<Func<'TEntity, bool>>
     -> DataSourceResult<'TDTO>
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dto"></param>
    abstract Insert : dto:'TDTO -> 'TDTO
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="ids"></param>
    abstract Update : dto:'TDTO * [<ParamArray>] ids:obj [] -> unit

and IListRepository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct> = 
    inherit IRepository<'TEntity, 'TDTO>
    /// <summary>
    /// 
    /// </summary>
    abstract ListRepository : IRepository<'TEntity, 'TListDTO> with get, set

and IUnitOfWork = 
    inherit IDisposable
    
    /// <summary>
    /// 
    /// </summary>
    abstract SaveChanges : unit -> int
    
    /// <summary>
    /// 
    /// </summary>
    abstract AfterSaveChanges : unit -> unit
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    abstract Entry<'TEntity> : entity:'TEntity -> DbEntityEntry
    
    /// <summary>
    /// 
    /// </summary>
    abstract DbSet<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null> : unit
     -> DbSet<'TEntity>
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="body"></param>
    abstract Transaction : body:Action<IUnitOfWork> -> unit
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="body"></param>
    abstract TransactionSaveChanges : body:Action<IUnitOfWork> -> bool
    
    /// <summary>
    /// 
    /// </summary>
    abstract CustomRepository<'TRepository when 'TRepository :> IRepository> : unit -> 'TRepository
    
    /// <summary>
    /// 
    /// </summary>
    abstract Repository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null> : unit
     -> IRepository<'TEntity>
    
    /// <summary>
    /// 
    /// </summary>
    abstract Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct> : unit
     -> IRepository<'TEntity, 'TDTO>
    
    /// <summary>
    /// 
    /// </summary>
    abstract Repository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct> : unit
     -> IListRepository<'TEntity, 'TDTO, 'TListDTO>

type IUnitOfWorkCaller = 
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    abstract UnitOfWork<'T> : call:Func<IUnitOfWork, 'T> -> 'T
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    abstract UnitOfWork : call:Action<IUnitOfWork> -> unit
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    abstract CustomRepository<'T, 'TRepository when 'TRepository :> IRepository> : call:Func<'TRepository, 'T> -> 'T
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    abstract Repository<'T, 'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null> : call:Func<IRepository<'TEntity>, 'T>
     -> 'T
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    abstract Repository<'T, 'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : not struct and 'TDTO : equality and 'TDTO : null> : call:Func<IRepository<'TEntity, 'TDTO>, 'T>
     -> 'T
    
    /// <summary>
    /// 
    /// </summary>
    abstract RepositoryList<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : not struct and 'TDTO : equality and 'TDTO : null> : unit
     -> seq<'TDTO>
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    abstract UnitOfWorkSaveChanges : call:Action<IUnitOfWork> -> bool
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="body"></param>
    abstract Transaction : body:Action<IUnitOfWork> -> unit
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="body"></param>
    abstract TransactionSaveChanges : body:Action<IUnitOfWork> -> bool
