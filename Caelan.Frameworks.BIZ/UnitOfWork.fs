namespace Caelan.Frameworks.BIZ.Classes

open Autofac
open System
open System.Collections
open System.Linq
open System.Data.Entity
open System.Reflection
open Caelan.Frameworks.BIZ.Interfaces
open Caelan.Frameworks.Common.Helpers

type UnitOfWork internal (context : DbContext, autoContext) as uow = 
    
    let mutable container = 
        let assemblies = 
            [| AssemblyHelper.GetWebEntryAssembly()
               Assembly.GetEntryAssembly()
               Assembly.GetCallingAssembly()
               Assembly.GetExecutingAssembly() |]
            |> Array.where (isNull >> not)
        
        let cb = ContainerBuilder()
        cb.Register<UnitOfWork>(fun u -> uow).AsSelf().As<UnitOfWork>().AsImplementedInterfaces() |> ignore
        cb.RegisterAssemblyTypes(assemblies).Where(fun t -> t.IsAssignableTo<IRepository>()).AsSelf()
          .AsImplementedInterfaces() |> ignore
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
    
    member __.SaveChangesAsync() = async { return! context.SaveChangesAsync() |> Async.AwaitTask } |> Async.StartAsTask
    
    member __.RegisterRepository<'TRepository when 'TRepository :> IRepository>(assemblies : Assembly []) = 
        if container.IsRegistered(typeof<'TRepository>) |> not then 
            let cb = ContainerBuilder()
            if container.IsRegistered(typeof<Repository>) |> not then 
                cb.RegisterType<Repository>().AsSelf().As<IRepository>() |> ignore
            if container.IsRegistered(typedefof<Repository<_>>) |> not then 
                cb.RegisterGeneric(typedefof<Repository<_>>).AsSelf().As(typedefof<IRepository<_>>) |> ignore
            if container.IsRegistered(typedefof<Repository<_, _>>) |> not then 
                cb.RegisterGeneric(typedefof<Repository<_, _>>).AsSelf().As(typedefof<IRepository<_, _>>) |> ignore
            if container.IsRegistered(typedefof<ListRepository<_, _, _>>) |> not then 
                cb.RegisterGeneric(typedefof<ListRepository<_, _, _>>).AsSelf().As(typedefof<IListRepository<_, _, _>>) 
                |> ignore
            let allAssemblies = 
                let getRefAssemblies (assembly : Assembly) = 
                    assembly.GetReferencedAssemblies() |> Array.map Assembly.Load
                
                let arr = 
                    [| typeof<'TRepository>.Assembly
                       AssemblyHelper.GetWebEntryAssembly()
                       Assembly.GetEntryAssembly()
                       Assembly.GetCallingAssembly()
                       Assembly.GetExecutingAssembly() |]
                    |> Array.append assemblies
                    |> Array.where (isNull >> not)
                arr
                |> Array.append (arr |> Array.collect getRefAssemblies)
                |> Array.where (isNull >> not)
            cb.RegisterAssemblyTypes(allAssemblies)
              .Where(fun t -> 
              t.IsAssignableTo<IRepository>() && t.IsInterface |> not && t.IsAbstract |> not 
              && container.IsRegistered(t) |> not).AsSelf().AsImplementedInterfaces() |> ignore
            if typeof<'TRepository>.IsInterface
               |> not
               && typeof<'TRepository>.IsAbstract |> not then 
                cb.RegisterType<'TRepository>().AsSelf().AsImplementedInterfaces() |> ignore
            cb.Update(container)
    
    member private this.GetRepository<'TRepository when 'TRepository :> IRepository>(assemblies : Assembly []) = 
        this.RegisterRepository<'TRepository>(assemblies)
        container.Resolve<'TRepository>()
    
    member this.CustomRepository<'TRepository when 'TRepository :> IRepository>() = 
        let assemblies = 
            [| typeof<'TRepository>.Assembly
               AssemblyHelper.GetWebEntryAssembly()
               Assembly.GetEntryAssembly()
               Assembly.GetCallingAssembly()
               Assembly.GetExecutingAssembly() |]
        this.GetRepository<'TRepository>(assemblies)
    
    member this.Repository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>() = 
        let assemblies = 
            [| typeof<'TEntity>.Assembly
               AssemblyHelper.GetWebEntryAssembly()
               Assembly.GetEntryAssembly()
               Assembly.GetCallingAssembly()
               Assembly.GetExecutingAssembly() |]
        this.GetRepository<IRepository<'TEntity>>(assemblies)
    
    member this.Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>() = 
        let assemblies = 
            [| typeof<'TEntity>.Assembly
               typeof<'TDTO>.Assembly
               AssemblyHelper.GetWebEntryAssembly()
               Assembly.GetEntryAssembly()
               Assembly.GetCallingAssembly()
               Assembly.GetExecutingAssembly() |]
        this.GetRepository<IRepository<'TEntity, 'TDTO>>(assemblies)
    
    member this.Repository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct>() = 
        let assemblies = 
            [| typeof<'TEntity>.Assembly
               typeof<'TDTO>.Assembly
               typeof<'TListDTO>.Assembly
               AssemblyHelper.GetWebEntryAssembly()
               Assembly.GetEntryAssembly()
               Assembly.GetCallingAssembly()
               Assembly.GetExecutingAssembly() |]
        this.GetRepository<IListRepository<'TEntity, 'TDTO, 'TListDTO>>(assemblies)
    
    member uow.SaveChanges() = 
        context.ChangeTracker.DetectChanges()
        let entitiesGroup = 
            context.ChangeTracker.Entries()
            |> Seq.map (fun entry -> entry.Entity)
            |> Array.ofSeq
            |> Array.groupBy (fun t -> t.GetType())
        
        let res = context.SaveChanges()
        for item in entitiesGroup do
            let (entityType, entities) = item
            let mHelper = uow.GetType().GetMethod("CallOnSaveChanges", BindingFlags.NonPublic ||| BindingFlags.Instance)
            mHelper.MakeGenericMethod([| entityType |]).Invoke(uow, [| entities |]) |> ignore
        res
    
    member private uow.CallOnSaveChanges<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>(entities : obj []) = 
        uow.Repository<'TEntity>().OnSaveChanges(entities.Cast<'TEntity>() |> Array.ofSeq)
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
            let res = this.SaveChanges() <> 0
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