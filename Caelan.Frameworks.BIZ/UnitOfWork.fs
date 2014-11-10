namespace Caelan.Frameworks.BIZ.Classes

open System
open System.Data.Entity
open System.Linq
open System.Reflection
open System.Collections.Generic
open Caelan.Frameworks.BIZ.Interfaces

[<AllowNullLiteral>]
type UnitOfWork internal (context : DbContext) = 
    let repositories = new Dictionary<Type, IRepository>()
    
    let findRepository (repoType : Type, implementation : unit -> IRepository) = 
        let dictRepo = repositories.SingleOrDefault(fun t -> t.Key.IsAssignableFrom(repoType) || repoType.IsAssignableFrom(t.Key))
        if dictRepo.Value = null then 
            let repo = implementation()
            repositories.Add(repo.GetType(), repo)
            repo
        else dictRepo.Value
    
    interface IUnitOfWork with
        member __.SaveChanges() = context.SaveChanges()
        
        member this.Repository<'TRepository when 'TRepository :> IRepository>() = 
            let repoType = typeof<'TRepository>
            
            let implementation = 
                (fun () -> 
                (match this.GetType().GetProperties(BindingFlags.Instance) 
                       |> Seq.tryFind (fun t -> t.PropertyType = repoType) with
                 | Some(repositoryProp) -> repositoryProp.GetValue(this)
                 | None -> Activator.CreateInstance(repoType, this)) :?> IRepository)
            ((repoType, implementation) |> findRepository) :?> 'TRepository
        
        member this.Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>() = 
            let repoType = typeof<IRepository<'TEntity, 'TDTO>>
            let implementation = 
                (fun () -> GenericRepository.CreateGenericRepository<'TEntity, 'TDTO>(this) :> IRepository)
            ((repoType, implementation) |> findRepository) :?> IRepository<'TEntity, 'TDTO>
        
        member this.Repository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct>() = 
            let repoType = typeof<IListRepository<'TEntity, 'TDTO, 'TListDTO>>
            let implementation = 
                (fun () -> 
                GenericRepository.CreateGenericListRepository<'TEntity, 'TDTO, 'TListDTO>(this) :> IRepository)
            ((repoType, implementation) |> findRepository) :?> IListRepository<'TEntity, 'TDTO, 'TListDTO>
        
        member __.Entry<'TEntity>(entity : 'TEntity) = context.Entry(entity)
        member __.DbSet<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>() = 
            context.Set<'TEntity>()
    
    member this.SaveChanges() = (this :> IUnitOfWork).SaveChanges()
    member this.Entry<'TEntity>(entity) = (this :> IUnitOfWork).Entry(entity)
    member this.Repository<'TRepository when 'TRepository :> IRepository>() = 
        (this :> IUnitOfWork).Repository<'TRepository>()
    member this.Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>() = 
        (this :> IUnitOfWork).Repository<'TEntity, 'TDTO>()
    member this.Repository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct>() = 
        (this :> IUnitOfWork).Repository<'TEntity, 'TDTO, 'TListDTO>()
    member __.SaveChangesAsync() = async { return! context.SaveChangesAsync() |> Async.AwaitTask } |> Async.StartAsTask
    
    interface IDisposable with
        member __.Dispose() = ()
    
    abstract Dispose : unit -> unit
    default this.Dispose() = (this :> IDisposable).Dispose()

type UnitOfWork<'TContext when 'TContext :> DbContext> private (context) = 
    inherit UnitOfWork(context)

    override this.Dispose() =
        context.Dispose()
        base.Dispose()

    new() =
        let context = Activator.CreateInstance<'TContext>()
        new UnitOfWork<'TContext>(context)