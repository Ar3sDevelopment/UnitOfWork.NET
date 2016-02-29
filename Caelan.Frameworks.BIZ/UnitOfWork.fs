namespace Caelan.Frameworks.BIZ.Classes

open Autofac
open Caelan.Frameworks.BIZ.Interfaces
open Caelan.Frameworks.Common.Helpers
open System
open System.Collections
open System.Collections.ObjectModel
open System.Data.Entity
open System.Data.Entity.Core.Objects
open System.Data.Entity.Infrastructure
open System.Linq
open System.Reflection

type UnitOfWork internal (context : DbContext, autoContext) as uow = 
    let assemblies = ObservableCollection<Assembly>()
    
    let mutable container = 
        let cb = ContainerBuilder()
        cb.Register<UnitOfWork>(fun u -> uow).AsSelf().As<UnitOfWork>().AsImplementedInterfaces() |> ignore
        cb.RegisterType<Repository>().AsSelf().As<IRepository>().PreserveExistingDefaults() |> ignore
        cb.RegisterGeneric(typedefof<Repository<_>>).AsSelf().As(typedefof<IRepository<_>>) |> ignore
        cb.RegisterGeneric(typedefof<Repository<_, _>>).AsSelf().As(typedefof<IRepository<_, _>>) |> ignore
        cb.RegisterGeneric(typedefof<ListRepository<_, _, _>>).AsSelf().As(typedefof<IListRepository<_, _, _>>) |> ignore
        cb.Build()
    
    let isRepository (t : Type) = 
        t.IsAssignableTo<IRepository>() && t.IsInterface |> not && t.IsAbstract |> not && t <> typeof<Repository> 
        && ((t.IsGenericType && t.GetType().GetGenericTypeDefinition() <> typedefof<Repository<_>> && t.GetType().GetGenericTypeDefinition() <> typedefof<Repository<_, _>> && t.GetType().GetGenericTypeDefinition() <> typedefof<ListRepository<_, _, _>>) 
            || t.IsGenericType |> not) && container.IsRegistered(t) |> not
    
    let registerAssembly (assemblyArr : Assembly []) = 
        let cb = ContainerBuilder()
        cb.RegisterAssemblyTypes(assemblyArr |> Array.filter (fun t -> t.GetTypes() |> Array.exists isRepository)).Where(fun t -> t |> isRepository).AsSelf().AsImplementedInterfaces() |> ignore
        cb.Update(container)
        assemblyArr
        |> Array.collect (fun t -> t.GetReferencedAssemblies())
        |> Array.map Assembly.Load
        |> Array.filter (assemblies.Contains >> not)
        |> Array.filter (fun t -> t.GetTypes() |> Array.exists isRepository)
        |> Array.iter assemblies.Add
    
    do 
        assemblies.CollectionChanged.Add(fun t -> 
            t.NewItems.Cast<Assembly>()
            |> Array.ofSeq
            |> registerAssembly)
        [| AssemblyHelper.GetWebEntryAssembly()
           Assembly.GetEntryAssembly()
           Assembly.GetCallingAssembly()
           Assembly.GetExecutingAssembly() |]
        |> Array.filter (isNull >> not)
        |> Array.filter (assemblies.Contains >> not)
        |> Array.filter (fun t -> t.GetTypes() |> Array.exists isRepository)
        |> Array.iter assemblies.Add
    
    member private __.autoContext = autoContext
    
    interface IUnitOfWork with
        member this.SaveChanges() = this.SaveChanges()
        member this.AfterSaveChanges() = this.AfterSaveChanges()
        member this.DbSet<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>() = this.DbSet<'TEntity>()
        member this.Entry<'TEntity>(entity) = this.Entry<'TEntity>(entity)
        member this.CustomRepository<'TRepository when 'TRepository :> IRepository>() = this.CustomRepository<'TRepository>()
        member this.Repository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>() = this.Repository<'TEntity>()
        member this.Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>() = this.Repository<'TEntity, 'TDTO>()
        member this.Repository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct>() = 
            this.Repository<'TEntity, 'TDTO, 'TListDTO>()
        member this.Transaction(body : Action<IUnitOfWork>) = this.Transaction(body)
        member this.TransactionSaveChanges(body : Action<IUnitOfWork>) = this.TransactionSaveChanges(body)
    
    interface IDisposable with
        member this.Dispose() = this.Dispose()
    
    member __.RegisterRepository<'TRepository when 'TRepository :> IRepository>() = 
        if not <| typeof<'TRepository>.IsInterface && not <| typeof<'TRepository>.IsAbstract && not <| container.IsRegistered(typeof<'TRepository>) then 
            let cb = ContainerBuilder()
            cb.RegisterType<'TRepository>().AsSelf().AsImplementedInterfaces() |> ignore
            cb.Update(container)
            [| typeof<'TRepository>.Assembly
               AssemblyHelper.GetWebEntryAssembly()
               Assembly.GetEntryAssembly()
               Assembly.GetCallingAssembly()
               Assembly.GetExecutingAssembly() |]
            |> Array.filter (isNull >> not)
            |> Array.filter (assemblies.Contains >> not)
            |> Array.filter (fun t -> t.GetTypes() |> Array.exists isRepository)
            |> Array.iter assemblies.Add
    
    member private this.GetRepository<'TRepository when 'TRepository :> IRepository>(repoAssemblies : Assembly []) = 
        repoAssemblies |> Array.iter assemblies.Add
        this.RegisterRepository<'TRepository>()
        container.Resolve<'TRepository>()
    
    member this.CustomRepository<'TRepository when 'TRepository :> IRepository>() = 
        let assemblies = 
            [| typeof<'TRepository>.Assembly
               AssemblyHelper.GetWebEntryAssembly()
               Assembly.GetEntryAssembly()
               Assembly.GetCallingAssembly()
               Assembly.GetExecutingAssembly() |]
            |> Array.filter (isNull >> not)
            |> Array.filter (assemblies.Contains >> not)
            |> Array.filter (fun t -> t.GetTypes() |> Array.exists isRepository)
        this.GetRepository<'TRepository>(assemblies)
    
    member this.Repository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>() = 
        let assemblies = 
            [| typeof<'TEntity>.Assembly
               AssemblyHelper.GetWebEntryAssembly()
               Assembly.GetEntryAssembly()
               Assembly.GetCallingAssembly()
               Assembly.GetExecutingAssembly() |]
            |> Array.filter (isNull >> not)
            |> Array.filter (assemblies.Contains >> not)
            |> Array.filter (fun t -> t.GetTypes() |> Array.exists isRepository)
        this.GetRepository<IRepository<'TEntity>>(assemblies)
    
    member this.Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>() = 
        let assemblies = 
            [| typeof<'TEntity>.Assembly
               typeof<'TDTO>.Assembly
               AssemblyHelper.GetWebEntryAssembly()
               Assembly.GetEntryAssembly()
               Assembly.GetCallingAssembly()
               Assembly.GetExecutingAssembly() |]
            |> Array.filter (isNull >> not)
            |> Array.filter (assemblies.Contains >> not)
            |> Array.filter (fun t -> t.GetTypes() |> Array.exists isRepository)
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
            |> Array.filter (isNull >> not)
            |> Array.filter (assemblies.Contains >> not)
            |> Array.filter (fun t -> t.GetTypes() |> Array.exists isRepository)
        this.GetRepository<IListRepository<'TEntity, 'TDTO, 'TListDTO>>(assemblies)
    
    member uow.SaveChanges() = 
        context.ChangeTracker.DetectChanges()
        let entitiesGroup = 
            context.ChangeTracker.Entries()
            |> Seq.filter (fun t -> t.State <> EntityState.Unchanged)
            |> Seq.map (fun entry -> entry.Entity)
            |> Array.ofSeq
            |> Array.groupBy (fun t -> ObjectContext.GetObjectType(t.GetType()))
        
        let res = context.SaveChanges()
        for item in entitiesGroup do
            let (entityType, entities) = item
            let mHelper = uow.GetType().GetMethod("CallOnSaveChanges", BindingFlags.NonPublic ||| BindingFlags.Instance)
            mHelper.MakeGenericMethod([| entityType |]).Invoke(uow, [| entities |]) |> ignore
        uow.AfterSaveChanges()
        res
    
    member this.SaveChangesAsync() = async { return this.SaveChanges() } |> Async.StartAsTask
    abstract AfterSaveChanges : unit -> unit
    override uow.AfterSaveChanges() = ()
    member private uow.CallOnSaveChanges<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>(entities : obj []) = uow.Repository<'TEntity>().OnSaveChanges(entities.Cast<'TEntity>() |> Array.ofSeq)
    member __.Entry<'TEntity>(entity : 'TEntity) = context.Entry(entity)
    member __.DbSet<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>() = context.Set<'TEntity>()
    
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