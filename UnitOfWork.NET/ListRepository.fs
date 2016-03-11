namespace UnitOfWork.NET.Classes

open UnitOfWork.NET.Interfaces

type ListRepository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct>(manager) = 
    inherit Repository<'TEntity, 'TDTO>(manager : IUnitOfWork)
    let mutable listRepository = manager.Repository<'TEntity, 'TListDTO>()
    
    interface IListRepository<'TEntity, 'TDTO, 'TListDTO> with
        member this.ListRepository 
            with get () = this.ListRepository
            and set (value) = this.ListRepository <- value

    member __.ListRepository 
        with get () = listRepository
        and set (value) = listRepository <- value