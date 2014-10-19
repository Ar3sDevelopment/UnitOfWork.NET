namespace Caelan.Frameworks.BIZ.Interfaces

open System.Data.Entity
open Caelan.Frameworks.BIZ.Classes

[<AllowNullLiteral>]
type IRepository = 
    abstract member GetUnitOfWork : unit -> IUnitOfWork

    abstract member GetUnitOfWork<'T when 'T :> IUnitOfWork> : unit -> 'T

and [<AllowNullLiteral>] IRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct> =
    (*inherit IRepository with
        abstract member GetUnitOfWork : unit -> IUnitOfWork
        abstract member GetUnitOfWork<'T when 'T :> IUnitOfWork> : unit -> 'T*)

    abstract member DTOBuilder : unit -> BaseDTOBuilder<'TEntity, 'TDTO>
    abstract member EntityBuilder: unit -> BaseEntityBuilder<'TDTO, 'TEntity>
    abstract member Set : unit -> DbSet<'TEntity>
    abstract member Single : ids : obj [] -> 'TDTO

and [<AllowNullLiteral>] IUnitOfWork =
    abstract member SaveChanges : unit -> int

    abstract member Repository<'TRepository when 'TRepository :> IRepository> : unit -> 'TRepository

    abstract member Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct> : unit -> IRepository<'TEntity, 'TDTO>

    abstract member Repository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct> : unit -> IRepository