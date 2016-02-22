namespace Caelan.Frameworks.BIZ.Classes

open Autofac
open System
open System.Data.Entity
open System.Reflection
open Caelan.Frameworks.BIZ.Interfaces
open Caelan.Frameworks.Common.Helpers
open Caelan.Frameworks.BIZ.Modules

type UnitOfWork internal (context : DbContext, autoContext) = 
    
    member private __.container = 
        let cb = ContainerBuilder()
        cb.RegisterType<UnitOfWork>().AsSelf().AsImplementedInterfaces() |> ignore
        cb.Build()
    
    member private __.autoContext = autoContext
    
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
    
    member uow.SaveChanges() = context.SaveChanges()
    member __.SaveChangesAsync() = async { return! context.SaveChangesAsync() |> Async.AwaitTask } |> Async.StartAsTask
    member this.CustomRepository<'TRepository when 'TRepository :> IRepository>() = this.GetRepository<'TRepository>()
    
    member private this.GetRepository<'TRepository when 'TRepository :> IRepository>() = 
        let mutable repository = Unchecked.defaultof<'TRepository>
        if this.container.TryResolve<'TRepository>(&repository) |> not then 
            let cb = ContainerBuilder()
            cb.RegisterType<'TRepository>().AsSelf().AsImplementedInterfaces() |> ignore
            cb.Update(this.container)
            use scope = this.container.BeginLifetimeScope()
            repository <- this.container.Resolve<'TRepository>()
        repository
    
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
    
    override this.Dispose() = 
        if this.autoContext then context.Dispose()
    
    new(context : DbContext) = new UnitOfWork(context, false)

type UnitOfWork<'TContext when 'TContext :> DbContext> private (context : DbContext) = 
    inherit UnitOfWork(context, true)
    new() = new UnitOfWork<'TContext>(Activator.CreateInstance<'TContext>())