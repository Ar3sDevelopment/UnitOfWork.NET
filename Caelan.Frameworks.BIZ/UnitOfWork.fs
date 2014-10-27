namespace Caelan.Frameworks.BIZ.Classes

open System
open System.Data.Entity
open System.Reflection
open System.Collections.Generic
open Caelan.Frameworks.BIZ.Interfaces

[<AllowNullLiteral>]
type UnitOfWork internal (context : DbContext) = 
    let repositories = new Dictionary<Type, IRepository>()
    
    interface IUnitOfWork with
        member __.SaveChanges() = context.SaveChanges()
        
        member this.Repository<'TRepository when 'TRepository :> IRepository>() = 
            let repoType = typeof<'TRepository>
            if repositories.ContainsKey(repoType) = false then 
                (match this.GetType().GetProperties(BindingFlags.Instance) 
                       |> Seq.tryFind (fun t -> t.PropertyType = repoType) with
                 | Some(repositoryProp) -> repositories.Add(repoType, repositoryProp.GetValue(this) :?> IRepository)
                 | None -> 
                     repositories.Add(typeof<'TRepository>, (Activator.CreateInstance(repoType, this)) :?> 'TRepository))
            repositories.[repoType] :?> 'TRepository
        
        member this.Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>() = 
            GenericRepository.CreateGenericRepository<'TEntity, 'TDTO>(this)
        member this.Repository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct>() = 
            GenericRepository.CreateGenericListRepository<'TEntity, 'TDTO, 'TListDTO>(this)
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
    
    member this.Dispose() = (this :> IDisposable).Dispose()

type UnitOfWork<'TContext when 'TContext :> DbContext>() = 
    inherit UnitOfWork(Activator.CreateInstance<'TContext>())
