namespace Caelan.Frameworks.BIZ.Interfaces

    open System
    open System.Data.Entity
    open Caelan.Frameworks.DAL.Interfaces

    [<AllowNullLiteral>]
    type IDTO = interface end

    [<AllowNullLiteral>]
    type IDTO<'TKey when 'TKey :> System.IEquatable<'TKey>> =
        inherit IDTO
        abstract member ID : 'TKey with get, set

    type IBaseRepository = interface end

    type IInsertRepository<'TDTO when 'TDTO :> IDTO> =
        abstract member Insert : dto : 'TDTO -> unit

    type IUpdateRepository<'TDTO when 'TDTO :> IDTO> =
        abstract member Update : dto : 'TDTO -> unit

    type IDeleteRepository<'TDTO when 'TDTO :> IDTO> =
        abstract member Delete : dto : 'TDTO -> unit

    type IUnitOfWork =
        abstract member SaveChanges : unit -> int

        abstract member GetDbSet<'TEntity, 'TDTO, 'TKey when 'TKey :> IEquatable<'TKey> and 'TEntity :> IEntity<'TKey>  and 'TEntity : not struct and 'TDTO :> IDTO<'TKey> and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TKey : equality> : repository : IBaseRepository -> DbSet<'TEntity>
        //abstract member GetRepository<'T when 'T :> IBaseRepository> : unit -> 'T