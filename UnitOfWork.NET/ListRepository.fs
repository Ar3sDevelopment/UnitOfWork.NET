namespace UnitOfWork.NET.Classes

open UnitOfWork.NET.Interfaces

type ListRepository<'TSource, 'TDestination, 'TListDestination when 'TSource : not struct and 'TDestination : not struct and 'TListDestination : not struct>(manager) = 
    inherit Repository<'TSource, 'TDestination>(manager : IUnitOfWork)
    let mutable listRepository = manager.Repository<'TSource, 'TListDestination>()
    
    interface IListRepository<'TSource, 'TDestination, 'TListDestination> with
        member this.ListRepository 
            with get () = this.ListRepository
            and set (value) = this.ListRepository <- value

    member __.ListRepository 
        with get () = listRepository
        and set (value) = listRepository <- value