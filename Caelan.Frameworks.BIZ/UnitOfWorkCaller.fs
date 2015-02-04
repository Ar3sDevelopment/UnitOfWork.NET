namespace Caelan.Frameworks.BIZ.Classes

open System
open System.Data.Entity
open Caelan.Frameworks.BIZ.Interfaces

type GenericUnitOfWorkCaller<'TUnitOfWork when 'TUnitOfWork :> IUnitOfWork and 'TUnitOfWork : (new : unit
                                                                                                    -> 'TUnitOfWork)> internal () = 
    let uow = new 'TUnitOfWork()
    
    interface IDisposable with
        member this.Dispose() = this.Dispose()
    
    interface IUnitOfWorkCaller<'TUnitOfWork> with
        member this.UnitOfWork<'T>(call : Func<IUnitOfWork, 'T>) = this.UnitOfWork(call)
        member this.UnitOfWork(call : Action<IUnitOfWork>) = this.UnitOfWork(call)
        member this.CustomRepository<'T, 'TRepository when 'TRepository :> IRepository>(call : Func<'TRepository, 'T>) = 
            this.CustomRepository(call)
        member this.Repository<'T, 'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>(call : Func<IRepository<'TEntity>, 'T>) = 
            this.Repository<'T, 'TEntity>(call)
        member this.Repository<'T, 'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : not struct and 'TDTO : equality and 'TDTO : null>(call : Func<IRepository<'TEntity, 'TDTO>, 'T>) = 
            this.Repository<'T, 'TEntity, 'TDTO>(call)
        member this.RepositoryList<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : not struct and 'TDTO : equality and 'TDTO : null>() = 
            this.RepositoryList<'TEntity, 'TDTO>()
        member this.UnitOfWorkCallSaveChanges(call : Action<IUnitOfWork>) = this.UnitOfWorkCallSaveChanges(call)
        member this.Transaction(body : Action<IUnitOfWork>) = this.Transaction(body)
        member this.TransactionSaveChanges(body : Action<IUnitOfWork>) = this.TransactionSaveChanges(body)
    
    member __.UnitOfWork<'T>(call : Func<IUnitOfWork, 'T>) = call.Invoke(uow)
    member __.UnitOfWork(call : Action<IUnitOfWork>) = call.Invoke(uow)
    member this.CustomRepository<'T, 'TRepository when 'TRepository :> IRepository>(call : Func<'TRepository, 'T>) = 
        this.UnitOfWork(fun t -> call.Invoke(t.CustomRepository<'TRepository>()))
    member this.Repository<'T, 'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>(call : Func<IRepository<'TEntity>, 'T>) = 
        this.UnitOfWork(fun t -> call.Invoke(t.Repository<'TEntity>()))
    member this.Repository<'T, 'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : not struct and 'TDTO : equality and 'TDTO : null>(call : Func<IRepository<'TEntity, 'TDTO>, 'T>) = 
        this.UnitOfWork(fun t -> call.Invoke(t.Repository<'TEntity, 'TDTO>()))
    member this.RepositoryList<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : not struct and 'TDTO : equality and 'TDTO : null>() = 
        this.Repository<seq<'TDTO>, 'TEntity, 'TDTO>(fun t -> t.List())
    
    member this.UnitOfWorkCallSaveChanges(call : Action<IUnitOfWork>) = 
        this.UnitOfWork(fun t -> 
            call.Invoke(t)
            t.SaveChanges() <> 0)
    
    member this.Transaction(body : Action<IUnitOfWork>) = this.UnitOfWork(fun t -> t.Transaction(body))
    member this.TransactionSaveChanges(body : Action<IUnitOfWork>) = 
        this.UnitOfWork(fun t -> t.TransactionSaveChanges(body))
    member __.Dispose() = uow.Dispose()

type UnitOfWorkCaller<'TContext when 'TContext :> DbContext> internal () = 
    class
        inherit GenericUnitOfWorkCaller<UnitOfWork<'TContext>>()
    end

type UnitOfWorkCaller private () = 
    static member Context<'TContext when 'TContext :> DbContext>() = new UnitOfWorkCaller<'TContext>()
    static member UnitOfWork<'TUnitOfWork when 'TUnitOfWork :> IUnitOfWork and 'TUnitOfWork : (new : unit
                                                                                                    -> 'TUnitOfWork)>() = 
        new GenericUnitOfWorkCaller<'TUnitOfWork>()
