namespace Caelan.Frameworks.BIZ.Interfaces

[<AllowNullLiteral>]
type IRepository = 
    interface
    end

[<AllowNullLiteral>]
type IUnitOfWork =
    abstract member SaveChanges : unit -> int

    abstract member Repository<'TRepository when 'TRepository :> IRepository> : unit -> 'TRepository

    abstract member Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct> : unit -> IRepository

    abstract member Repository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct> : unit -> IRepository