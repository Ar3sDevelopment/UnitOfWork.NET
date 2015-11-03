namespace Caelan.Frameworks.BIZ.Classes

open System
open System.Data.Entity
open System.Reflection
open Caelan.Frameworks.BIZ.Interfaces
open Caelan.Frameworks.Common.Helpers
open Caelan.Frameworks.BIZ.Modules

type UnitOfWork internal (context : DbContext, autoContext) =
    member private __.AutoContext = autoContext

    interface IUnitOfWork with
        member this.SaveChanges() = this.SaveChanges()
        member this.DbSet<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>() = 
            this.DbSet<'TEntity>()
        member this.Entry<'TEntity>(entity) = this.Entry<'TEntity>(entity)
        member this.CustomRepository<'TRepository when 'TRepository :> IRepository>() = 
            this.CustomRepository<'TRepository>()
        member this.Repository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>() = 
            this.Repository<'TEntity>()
        member this.Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>() = 
            this.Repository<'TEntity, 'TDTO>()
        member this.Repository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct>() = 
            this.Repository<'TEntity, 'TDTO, 'TListDTO>()
        member this.Transaction(body : Action<IUnitOfWork>) = this.Transaction(body)
        member this.TransactionSaveChanges(body : Action<IUnitOfWork>) = this.TransactionSaveChanges(body)

    interface IDisposable with
        member this.Dispose() = this.Dispose()
    
    member __.SaveChanges() = context.SaveChanges()
    member __.SaveChangesAsync() = async { return! context.SaveChangesAsync() |> Async.AwaitTask } |> Async.StartAsTask
    
    member this.CustomRepository<'TRepository when 'TRepository :> IRepository>() = 
        typeof<'TRepository>
        |> MemoizeHelper.Memoize(fun tp -> 
                                    let repository =
                                        match this.GetType().GetProperties(BindingFlags.Instance) |> Seq.tryFind (fun t -> t.PropertyType = tp) with
                                        | Some(repositoryProp) -> repositoryProp.GetValue(this)
                                        | None -> Activator.CreateInstance(tp, this)
                                    repository :?> 'TRepository)
    
     member private this.GetRepository<'TRepository when 'TRepository :> IRepository>()=
        typeof<'TRepository> 
        |> MemoizeHelper.Memoize (fun t -> t |> RepositoryReflection.FindRepositoryInAssemblies<'TRepository> [| this |])
    member this.Repository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>() = 
        this.GetRepository<IRepository<'TEntity>>()
    member this.Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>() = 
        this.GetRepository<IRepository<'TEntity, 'TDTO>>()
    member this.Repository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct>() = 
        this.GetRepository<IListRepository<'TEntity, 'TDTO, 'TListDTO>>()
    
    member __.Entry<'TEntity>(entity : 'TEntity) = context.Entry(entity)
    member __.DbSet<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>() = 
        context.Set<'TEntity>()
    
    member this.Transaction(body : Action<IUnitOfWork>) = 
        use transaction = context.Database.BeginTransaction()
        try 
            this |> body.Invoke
            transaction.Commit()
        with _ -> transaction.Rollback()
    
    member this.TransactionSaveChanges(body : Action<IUnitOfWork>) = 
        use transaction = context.Database.BeginTransaction()
        try 
            this |> body.Invoke
            let res = context.SaveChanges() <> 0
            transaction.Commit()
            res
        with _ -> 
            transaction.Rollback()
            false
    
    abstract Dispose : unit -> unit
    override this.Dispose() = if this.AutoContext then context.Dispose()

    new(context: DbContext) = new UnitOfWork(context, false)

type UnitOfWork<'TContext when 'TContext :> DbContext> private (context : DbContext) = 
    inherit UnitOfWork(context, true)
    
    new() = new UnitOfWork<'TContext>(Activator.CreateInstance<'TContext>())