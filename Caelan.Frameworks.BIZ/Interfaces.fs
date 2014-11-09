namespace Caelan.Frameworks.BIZ.Interfaces

open System
open System.Data.Entity
open System.Data.Entity.Infrastructure
open System.Linq
open System.Linq.Expressions
open Caelan.Frameworks.Common.Classes
open Caelan.DynamicLinq.Classes
open Caelan.Frameworks.Common.Interfaces

[<AllowNullLiteral>]
type IRepository = 
    abstract GetUnitOfWork : unit -> IUnitOfWork
    abstract GetUnitOfWork<'T when 'T :> IUnitOfWork> : unit -> 'T

and [<AllowNullLiteral>] IRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct> = 
    inherit IRepository
    abstract DTOBuilder : IMapper<'TEntity, 'TDTO> -> Builder<'TEntity, 'TDTO>
    abstract EntityBuilder : IMapper<'TDTO, 'TEntity> -> Builder<'TDTO, 'TEntity>
    abstract DTOBuilder : unit -> Builder<'TEntity, 'TDTO>
    abstract EntityBuilder : unit -> Builder<'TDTO, 'TEntity>
    abstract Set : unit -> DbSet<'TEntity>
    abstract Single : [<ParamArray>]ids:obj [] -> 'TDTO
    abstract Single : Expression<Func<'TEntity, bool>> -> 'TDTO
    abstract List : unit -> seq<'TDTO>
    abstract List : Expression<Func<'TEntity, bool>> -> seq<'TDTO>
    abstract All : unit -> IQueryable<'TEntity>
    abstract All : Expression<Func<'TEntity, bool>> -> IQueryable<'TEntity>
    abstract All : int * int * seq<Sort> * Filter * Expression<Func<'TEntity, bool>> -> DataSourceResult<'TDTO>
    abstract Insert : 'TDTO -> unit
    abstract Insert : 'TEntity -> unit
    abstract Update : 'TDTO * [<ParamArray>]ids:obj [] -> unit
    abstract Update : 'TEntity * [<ParamArray>]ids:obj [] -> unit
    abstract Delete : 'TDTO * [<ParamArray>]ids:obj [] -> unit
    abstract Delete : 'TEntity * [<ParamArray>]ids:obj [] -> unit
    abstract Delete : [<ParamArray>]ids:obj [] -> unit

and [<AllowNullLiteral>] IListRepository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct> = 
    inherit IRepository<'TEntity, 'TDTO>
    abstract ListRepository : IRepository<'TEntity, 'TListDTO> with get, set

and [<AllowNullLiteral>] IUnitOfWork = 
    inherit IDisposable
    abstract SaveChanges : unit -> int
    abstract Entry<'TEntity> : 'TEntity -> DbEntityEntry
    abstract DbSet<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null> : unit
     -> DbSet<'TEntity>
    abstract Repository<'TRepository when 'TRepository :> IRepository> : unit -> 'TRepository
    abstract Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct> : unit
     -> IRepository<'TEntity, 'TDTO>
    abstract Repository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct> : unit
     -> IListRepository<'TEntity, 'TDTO, 'TListDTO>

type IUnitOfWorkCaller<'TUnitOfWork when 'TUnitOfWork :> IUnitOfWork and 'TUnitOfWork : (new : unit -> 'TUnitOfWork)> =
    abstract UnitOfWork<'T> : call: Func<IUnitOfWork, 'T> -> 'T
    abstract UnitOfWork : call: Action<IUnitOfWork> -> unit
    abstract Repository<'T, 'TRepository when 'TRepository :> IRepository> : call: Func<'TRepository, 'T> -> 'T
    abstract Repository<'T, 'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : not struct and 'TDTO : equality and 'TDTO : null> : call: Func<IRepository<'TEntity, 'TDTO>, 'T> -> 'T
    abstract RepositoryList<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : not struct and 'TDTO : equality and 'TDTO : null> : unit -> seq<'TDTO>
    abstract UnitOfWorkCallSaveChanges : call: Action<IUnitOfWork> -> bool