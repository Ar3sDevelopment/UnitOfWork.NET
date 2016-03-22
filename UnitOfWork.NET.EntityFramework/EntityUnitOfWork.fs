namespace UnitOfWork.NET.EntityFramework.Classes

open Autofac
open Caelan.Frameworks.Common.Helpers
open System
open System.Collections
open System.Collections.Generic
open System.Collections.ObjectModel
open System.Data.Entity
open System.Data.Entity.Core.Objects
open System.Data.Entity.Infrastructure
open System.Linq
open System.Reflection
open UnitOfWork.NET.Interfaces
open UnitOfWork.NET.Classes
open UnitOfWork.NET.EntityFramework.Interfaces

type EntityUnitOfWork internal (context : DbContext, autoContext) = 
    inherit UnitOfWork()

    member private __.autoContext = autoContext
    
    interface IEntityUnitOfWork with
        member this.BeforeSaveChanges context = this.BeforeSaveChanges context
        member this.SaveChanges() = this.SaveChanges()
        member this.AfterSaveChanges context = this.AfterSaveChanges context
        member this.Entry<'TEntity>(entity) = this.Entry<'TEntity>(entity)
        member this.CustomRepository<'TRepository when 'TRepository :> IRepository>() = this.CustomRepository<'TRepository>()
        member this.EntityRepository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>() = this.EntityRepository<'TEntity>()
        member this.EntityRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>() = this.EntityRepository<'TEntity, 'TDTO>()
        member this.Transaction(body : Action<IEntityUnitOfWork>) = this.Transaction(body)
        member this.TransactionSaveChanges(body : Action<IEntityUnitOfWork>) = this.TransactionSaveChanges(body)
    
    interface IDisposable with
        member this.Dispose() = this.Dispose()
    
    override this.Data<'T when 'T : not struct>() = context.Set<'T>().AsEnumerable()

    member this.EntityRepository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>() = 
        this.Repository<'TEntity>() :?> IEntityRepository<'TEntity>
    member this.EntityRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>() = 
        this.Repository<'TEntity, 'TDTO>() :?> IEntityRepository<'TEntity, 'TDTO>
    
    member uow.SaveChanges() = 
        context.ChangeTracker.DetectChanges()
        let entries = context.ChangeTracker.Entries()
        let entriesGroup = 
            entries
            |> Seq.filter (fun t -> t.State <> EntityState.Unchanged && t.State <> EntityState.Detached)
            |> Array.ofSeq
            |> Array.groupBy (fun t -> ObjectContext.GetObjectType(t.Entity.GetType()))
        
        let entitiesGroup = (entriesGroup |> Array.map (fun (t, e) -> (t, e.ToList().GroupBy((fun i -> i.State), (fun (i : DbEntityEntry) -> i.Entity)).ToList()))).ToList()
        context |> uow.BeforeSaveChanges
        let res = context.SaveChanges()
        for item in entitiesGroup do
            let (entityType, entitiesByState) = item
            let mHelper = uow.GetType().GetMethod("CallOnSaveChanges", BindingFlags.NonPublic ||| BindingFlags.Instance)
            mHelper.MakeGenericMethod([| entityType |]).Invoke(uow, [| entitiesByState.ToDictionary((fun t -> t.Key), (fun (t : IGrouping<EntityState, obj>) -> t.AsEnumerable())) |]) |> ignore
        context |> uow.AfterSaveChanges
        res
    
    member this.SaveChangesAsync() = async { return this.SaveChanges() } |> Async.StartAsTask
    abstract BeforeSaveChanges : context:DbContext -> unit
    override uow.BeforeSaveChanges context = ()
    abstract AfterSaveChanges : context:DbContext -> unit
    override uow.AfterSaveChanges context = ()
    
    member private uow.CallOnSaveChanges<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>(entitiesObj : Dictionary<EntityState, IEnumerable<obj>>) = 
        let entities = entitiesObj.ToDictionary((fun t -> t.Key), (fun (t : KeyValuePair<EntityState, IEnumerable<obj>>) -> t.Value.Cast<'TEntity>()))
        uow.EntityRepository<'TEntity>().OnSaveChanges(entities)
    
    member __.Entry<'TEntity>(entity : 'TEntity) = context.Entry(entity)
    member __.DbSet<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>() = context.Set<'TEntity>()
    
    member this.Transaction(body : Action<IEntityUnitOfWork>) = 
        use transaction = context.Database.BeginTransaction()
        try 
            this |> body.Invoke
            transaction.Commit()
        with _ -> transaction.Rollback()
    
    member this.TransactionSaveChanges(body : Action<IEntityUnitOfWork>) = 
        use transaction = context.Database.BeginTransaction()
        try 
            this |> body.Invoke
            let res = this.SaveChanges() <> 0
            transaction.Commit()
            res
        with _ -> 
            transaction.Rollback()
            false
    
    override this.Dispose() = 
        if this.autoContext then context.Dispose()
    
    new(context : DbContext) = new EntityUnitOfWork(context, false)

type EntityUnitOfWork<'TContext when 'TContext :> DbContext> private (context : DbContext) = 
    inherit EntityUnitOfWork(context, true)
    new() = new EntityUnitOfWork<'TContext>(Activator.CreateInstance<'TContext>())
    member uow.BeforeSaveChanges(context : DbContext) = base.BeforeSaveChanges context
    member uow.AfterSaveChanges(context : DbContext) = base.AfterSaveChanges context
    abstract BeforeSaveChanges : context:'TContext -> unit
    override uow.BeforeSaveChanges(context : 'TContext) = uow.BeforeSaveChanges(context :> DbContext)
    abstract AfterSaveChanges : context:'TContext -> unit
    override uow.AfterSaveChanges(context : 'TContext) = uow.AfterSaveChanges(context :> DbContext)