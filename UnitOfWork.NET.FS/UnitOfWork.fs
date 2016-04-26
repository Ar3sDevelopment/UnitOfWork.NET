namespace UnitOfWork.NET.FS.Classes

open Autofac
open Caelan.Frameworks.Common.Helpers
open System
open System.Collections
open System.Collections.Generic
open System.Collections.ObjectModel
open System.Linq
open System.Reflection
open UnitOfWork.NET.FS.Interfaces

type UnitOfWork() as uow = 
    let assemblies = ObservableCollection<Assembly>()
    
    let mutable container = 
        let cb = ContainerBuilder()
        cb.Register(fun t -> uow).AsImplementedInterfaces().AsSelf().As<UnitOfWork>() |> ignore
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
        member this.Data<'T when 'T : not struct>() = this.Data<'T>()
        member this.CustomRepository<'TRepository when 'TRepository :> IRepository>() = this.CustomRepository<'TRepository>()
        member this.Repository<'T when 'T : not struct>() = this.Repository<'T>()
        member this.Repository<'TSource, 'TDestination when 'TSource : not struct and 'TDestination : not struct>() = this.Repository<'TSource, 'TDestination>()
        member this.Repository<'TSource, 'TDestination, 'TListDestination when 'TSource : not struct and 'TDestination : not struct and 'TListDestination : not struct>() = this.Repository<'TSource, 'TDestination, 'TListDestination>()
    
    interface IDisposable with
        member this.Dispose() = this.Dispose()
    
    member this.RegisterRepository<'TRepository when 'TRepository :> IRepository>() = 
        this.RegisterRepository(typeof<'TRepository>)

    member __.RegisterRepository repositoryType =
        if not <| repositoryType.IsInterface && not <| repositoryType.IsAbstract && not <| container.IsRegistered(repositoryType) then 
            let cb = ContainerBuilder()
            if repositoryType.IsGenericTypeDefinition then
                cb.RegisterGeneric(repositoryType).AsSelf().AsImplementedInterfaces() |> ignore
            else    
                cb.RegisterType(repositoryType).AsSelf().AsImplementedInterfaces() |> ignore
            cb.Update(container)
    
    member private this.GetRepository<'TRepository when 'TRepository :> IRepository>() = 
        this.RegisterRepository<'TRepository>()
        container.Resolve<'TRepository>()
    
    member this.CustomRepository<'TRepository when 'TRepository :> IRepository>() = this.GetRepository<'TRepository>()
    member this.Repository<'T when 'T : not struct>() = this.GetRepository<IRepository<'T>>()
    member this.Repository<'TSource, 'TDestination when 'TSource : not struct and 'TDestination : not struct>() = this.GetRepository<IRepository<'TSource, 'TDestination>>()
    member this.Repository<'TSource, 'TDestination, 'TListDestination when 'TSource : not struct and 'TDestination : not struct and 'TListDestination : not struct>() = this.GetRepository<IListRepository<'TSource, 'TDestination, 'TListDestination>>()
    abstract Data<'T when 'T : not struct> : unit -> seq<'T>
    override __.Data<'T when 'T : not struct>() = Seq.empty<'T>
    
    abstract Dispose : unit -> unit
    override this.Dispose() = ()
