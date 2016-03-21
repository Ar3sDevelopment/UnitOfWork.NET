namespace UnitOfWork.NET.Classes

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

[<AbstractClass>]
type UnitOfWork() as uow = 
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
        try 
            t.IsAssignableTo<IRepository>() && t.IsInterface |> not && t.IsAbstract |> not && t <> typeof<Repository> 
            && ((t.IsGenericType && t.GetGenericTypeDefinition() <> typedefof<Repository<_>> && t.GetGenericTypeDefinition() <> typedefof<Repository<_, _>> && t.GetGenericTypeDefinition() <> typedefof<ListRepository<_, _, _>>) || t.IsGenericType |> not) 
            && container.IsRegistered(t) |> not
        with _ -> false
    
    let registerAssembly (assemblyArr : Assembly []) = 
        let cb = ContainerBuilder()
        cb.RegisterAssemblyTypes(assemblyArr |> Array.filter (fun t -> 
                                                    try 
                                                        t.GetTypes() |> Array.exists isRepository
                                                    with _ -> false)).Where(fun t -> t |> isRepository).AsSelf().AsImplementedInterfaces()
        |> ignore
        cb.Update(container)
        assemblyArr
        |> Array.collect (fun t -> t.GetReferencedAssemblies())
        |> Array.map (fun t -> 
               try 
                   t |> Assembly.Load
               with _ -> null)
        |> Array.filter (isNull >> not)
        |> Array.filter (assemblies.Contains >> not)
        |> Array.filter (fun t -> 
               try 
                   t.GetTypes() |> Array.exists isRepository
               with _ -> false)
        |> Array.iter assemblies.Add
    
    do 
        let cb = ContainerBuilder()
        let fields = uow.GetType().GetFields().Where(fun t -> t.FieldType |> isRepository) |> Array.ofSeq
        fields |> Array.iter (fun t -> cb.RegisterType(t.FieldType).AsSelf().AsImplementedInterfaces() |> ignore)
        let properties = uow.GetType().GetProperties().Where(fun t -> t.PropertyType |> isRepository) |> Array.ofSeq
        properties |> Array.iter (fun t -> cb.RegisterType(t.PropertyType).AsSelf().AsImplementedInterfaces() |> ignore)
        cb.Update(container)
        fields |> Array.iter (fun t -> t.SetValue(uow, container.ResolveOptional(t.FieldType)))
        properties |> Array.iter (fun t -> t.SetValue(uow, container.ResolveOptional(t.PropertyType)))
        assemblies.CollectionChanged.Add(fun t -> 
            t.NewItems.Cast<Assembly>()
            |> Array.ofSeq
            |> registerAssembly)
        AppDomain.CurrentDomain.GetAssemblies() |> Array.iter assemblies.Add
    
    interface IUnitOfWork with
        member this.SaveChanges() = this.SaveChanges()
        member this.Data<'T>() = this.Data<'T>()
        member this.CustomRepository<'TRepository when 'TRepository :> IRepository>() = this.CustomRepository<'TRepository>()
        member this.Repository<'T>() = this.Repository<'T>()
        member this.Repository<'TSource, 'TDestination>() = this.Repository<'TSource, 'TDestination>()
        member this.Repository<'TSource, 'TDestination, 'TListDestination>() = this.Repository<'TSource, 'TDestination, 'TListDestination>()
        member this.Transaction(body : Action<IUnitOfWork>) = this.Transaction(body)
        member this.TransactionSaveChanges(body : Action<IUnitOfWork>) = this.TransactionSaveChanges(body)
    
    interface IDisposable with
        member this.Dispose() = this.Dispose()
    
    member __.RegisterRepository<'TRepository when 'TRepository :> IRepository>() = 
        if not <| typeof<'TRepository>.IsInterface && not <| typeof<'TRepository>.IsAbstract && not <| container.IsRegistered(typeof<'TRepository>) then 
            let cb = ContainerBuilder()
            cb.RegisterType<'TRepository>().AsSelf().AsImplementedInterfaces() |> ignore
            cb.Update(container)
    
    member private this.GetRepository<'TRepository when 'TRepository :> IRepository>() = 
        this.RegisterRepository<'TRepository>()
        container.Resolve<'TRepository>()
    
    member this.CustomRepository<'TRepository when 'TRepository :> IRepository>() = this.GetRepository<'TRepository>()
    member this.Repository<'T>() = this.GetRepository < IRepository<'T>()
    member this.Repository<'TSource, 'TDestination>() = this.GetRepository<IRepository<'TSource, 'TDestination>>()
    member this.Repository<'TSource, 'TDestination, 'TListDestination>() = this.GetRepository<IListRepository<'TSource, 'TDestination, 'TListDestination>>()
    member uow.SaveChanges() = ()
    member this.SaveChangesAsync() = async { return this.SaveChanges() } |> Async.StartAsTask
    abstract Data<'T> : unit -> seq<'T>
    member this.Transaction(body : Action<IUnitOfWork>) = this |> body.Invoke
    
    member this.TransactionSaveChanges(body : Action<IUnitOfWork>) = 
        this |> body.Invoke
        this.SaveChanges()
    
    abstract Dispose : unit -> unit
    override this.Dispose() = ()
