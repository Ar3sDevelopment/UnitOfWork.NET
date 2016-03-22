namespace UnitOfWork.NET.EntityFramework.Interfaces

open System
open System.Collections.Generic
open System.Data.Entity
open System.Data.Entity.Infrastructure
open System.Linq
open System.Linq.Expressions
open Caelan.DynamicLinq.Classes
open UnitOfWork.NET.Interfaces

type IEntityRepository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null> = 
    inherit IRepository<'TEntity>
    
    /// <summary>
    /// The DbSet used by Repository to perform operations on database.
    /// </summary>
    abstract Set : DbSet<'TEntity>
    
    /// <summary>
    /// Recovers the entity by primary keys.
    /// </summary>
    /// <param name="ids">The primary keys values</param>
    abstract Entity : [<ParamArray>] ids:obj [] -> 'TEntity
    
    /// <summary>
    /// Recovers an entity using a where, if multiple returned it returns the first.
    /// </summary>
    /// <param name="where">The condition for retrieving the entity</param>
    abstract Entity : where:Expression<Func<'TEntity, bool>> -> 'TEntity
    
    /// <summary>
    /// All entities of the DbSet filtered by the where.
    /// </summary>
    /// <param name="where">The condition for filtering all entities</param>
    abstract All : where:Expression<Func<'TEntity, bool>> -> IEnumerable<'TEntity>
    
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
    abstract OnSaveChanges : entities:IDictionary<EntityState, IEnumerable<'TEntity>> -> unit

and IEntityRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct> = 
    inherit IEntityRepository<'TEntity>
    inherit IRepository<'TEntity, 'TDTO>
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ids"></param>
    abstract DTO : [<ParamArray>] ids:obj [] -> 'TDTO
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="where"></param>
    abstract DTO : where:Expression<Func<'TEntity, bool>> -> 'TDTO
    
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
    abstract DataSource : take:int * skip:int * sort:ICollection<Sort> * filter:Filter * where:Expression<Func<'TEntity, bool>> -> DataSourceResult<'TDTO>
    
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

and IEntityUnitOfWork = 
    inherit IUnitOfWork
    
    /// <summary>
    /// 
    /// </summary>
    abstract BeforeSaveChanges : context:DbContext -> unit
    
    /// <summary>
    /// 
    /// </summary>
    abstract SaveChanges : unit -> int
    
    /// <summary>
    /// 
    /// </summary>
    abstract AfterSaveChanges : context:DbContext -> unit
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    abstract Entry<'TEntity> : entity:'TEntity -> DbEntityEntry
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="body"></param>
    abstract Transaction : body:Action<IEntityUnitOfWork> -> unit
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="body"></param>
    abstract TransactionSaveChanges : body:Action<IEntityUnitOfWork> -> bool
    
    /// <summary>
    /// 
    /// </summary>
    abstract EntityRepository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null> : unit -> IEntityRepository<'TEntity>
    
    /// <summary>
    /// 
    /// </summary>
    abstract EntityRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct> : unit -> IEntityRepository<'TEntity, 'TDTO>