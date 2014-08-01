namespace Caelan.Frameworks.BIZ.Classes

open System
open System.Data.Entity
open System.Runtime.CompilerServices
open System.Collections.Generic
open System.Linq
open System.Linq.Expressions
open System.Reflection
open AutoMapper
open AutoMapper.Internal
open Caelan.Frameworks.Common.Classes
open Caelan.Frameworks.Common.Extenders
open Caelan.Frameworks.BIZ.Interfaces
open Caelan.DynamicLinq.Classes
open Caelan.DynamicLinq.Extensions

[<AbstractClass>]
[<AllowNullLiteral>]
type BaseRepository(manager) = 
    interface IBaseRepository
    member private __.UnitOfWork : BaseUnitOfWork = manager
    member this.GetUnitOfWork() = this.UnitOfWork
    member this.GetUnitOfWork<'T when 'T :> BaseUnitOfWork>() = this.UnitOfWork :?> 'T

and [<AllowNullLiteral>] BaseRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>(manager) = 
    inherit BaseRepository(manager)
    abstract Set : unit -> DbSet<'TEntity>
    override this.Set() = this.GetUnitOfWork().DbSet(this)
    abstract List : unit -> IEnumerable<'TDTO>
    override this.List() = this.DTOBuilder().BuildList(this.All())
    abstract List : whereExpr:Expression<Func<'TEntity, bool>> -> IEnumerable<'TDTO>
    override this.List(whereExpr) = this.DTOBuilder().BuildList(this.All(whereExpr))
    abstract All : unit -> IQueryable<'TEntity>
    override this.All() = this.Set() :> IQueryable<'TEntity>
    abstract All : whereExpr:Expression<Func<'TEntity, bool>> -> IQueryable<'TEntity>
    
    override this.All(whereExpr) = 
        match whereExpr with
        | null -> this.All()
        | _ -> this.Set().Where(whereExpr)
    
    abstract All : int * int * seq<Sort> * Filter * Expression<Func<'TEntity, bool>> -> DataSourceResult<'TDTO>
    
    override this.All(take, skip, sort, filter, whereFunc) =
        let defaultSort = (typedefof<'TEntity>).GetProperties(BindingFlags.Instance ||| BindingFlags.Public).Select(fun t -> t.Name).FirstOrDefault()
        let all =
            match defaultSort with
            | null -> this.All(whereFunc)
            | _ -> this.All(whereFunc).OrderBy(defaultSort)
        let queryResult = all.ToDataSourceResult(take, skip, sort, filter)
        let result = DataSourceResult<'TDTO>()
        result.Data <- this.DTOBuilder().BuildList(queryResult.Data)
        result.Total <- queryResult.Total
        result
    
    abstract AllFull : int * int * seq<Sort> * Filter * Expression<Func<'TEntity, bool>> -> DataSourceResult<'TDTO>
    
    override this.AllFull(take, skip, sort, filter, whereFunc) = 
        let queryResult = this.All(whereFunc).ToDataSourceResult(take, skip, sort, filter)
        let result = DataSourceResult<'TDTO>()
        result.Data <- this.DTOBuilder().BuildFullList(queryResult.Data)
        result.Total <- queryResult.Total
        result
    
    abstract DTOBuilder : unit -> BaseDTOBuilder<'TEntity, 'TDTO>
    override __.DTOBuilder() = GenericBusinessBuilder.GenericDTOBuilder<'TEntity, 'TDTO>()
    abstract EntityBuilder : unit -> BaseEntityBuilder<'TDTO, 'TEntity>
    override __.EntityBuilder() = GenericBusinessBuilder.GenericEntityBuilder<'TDTO, 'TEntity>()
    abstract Single : obj [] -> 'TDTO
    override this.Single([<ParamArray>] ids) = this.DTOBuilder().BuildFull(this.Set().Find(ids))
    abstract Single : Expression<Func<'TEntity, bool>> -> 'TDTO
    override this.Single(expr) = this.DTOBuilder().BuildFull(this.Set().FirstOrDefault(expr))
    member this.ListAsync(whereExpr) = async { return this.List(whereExpr) } |> Async.StartAsTask
    member this.ListAsync() = async { return this.List() } |> Async.StartAsTask
    member this.AllAsync(take, skip, sort, filter, whereFunc) = 
        async { return this.All(take, skip, sort, filter, whereFunc) } |> Async.StartAsTask
    member this.SingleAsync([<ParamArray>] id : obj []) = async { return this.Single(id) } |> Async.StartAsTask
    member this.SingleAsync(expr : Expression<Func<'TEntity, bool>>) = 
        async { return this.Single(expr) } |> Async.StartAsTask

and [<AbstractClass>] BaseUnitOfWork internal (context : DbContext) = 
    member internal __.DbSet<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>(repository : BaseRepository<'TEntity, 'TDTO>) = 
        context.Set<'TEntity>()
    member __.SaveChanges() = context.SaveChanges()
    member __.SaveChangesAsync() = async { return! context.SaveChangesAsync() |> Async.AwaitTask } |> Async.StartAsTask
    member __.Entry<'TEntity>(entity : 'TEntity) = context.Entry(entity)
    
    member this.Repository<'TRepository when 'TRepository :> BaseRepository>() = 
        try 
            let repositoryProp = 
                this.GetType().GetProperties(BindingFlags.Instance) 
                |> Seq.find (fun t -> t.PropertyType = typedefof<'TRepository>)
            repositoryProp.GetValue(this) :?> 'TRepository
        with :? KeyNotFoundException -> Activator.CreateInstance(typedefof<'TRepository>, this) :?> 'TRepository
    
    member this.Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>() = 
        let repoType = typeof<BaseRepository<'TEntity, 'TDTO>>
        try 
            let repositoryProp = 
                this.GetType().GetProperties(BindingFlags.Instance) 
                |> Seq.find (fun t -> t.PropertyType.BaseType = repoType)
            repositoryProp.GetValue(this) :?> BaseRepository<'TEntity, 'TDTO>
        with :? KeyNotFoundException -> 
            (match repoType.IsGenericTypeDefinition with
             | true -> 
                 Activator.CreateInstance
                     (repoType.MakeGenericType(typedefof<'TEntity>, typedefof<'TDTO>), this)
             | _ -> Activator.CreateInstance(repoType, this)) :?> BaseRepository<'TEntity, 'TDTO>
    
    member this.CRUDRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>() = 
        let repoType = typeof<BaseCRUDRepository<'TEntity, 'TDTO>>
        try 
            let repositoryProp = 
                this.GetType().GetProperties(BindingFlags.Instance) 
                |> Seq.find (fun t -> t.PropertyType.BaseType = repoType)
            repositoryProp.GetValue(this) :?> BaseCRUDRepository<'TEntity, 'TDTO>
        with :? KeyNotFoundException -> 
            (match repoType.IsGenericTypeDefinition with
             | true -> 
                 Activator.CreateInstance
                     (repoType.MakeGenericType(typedefof<'TEntity>, typedefof<'TDTO>), this)
             | _ -> Activator.CreateInstance(repoType, this)) :?> BaseCRUDRepository<'TEntity, 'TDTO>
    
    member __.Dispose() = ()
    interface IDisposable with
        member this.Dispose() = this.Dispose()

and [<AbstractClass>] BaseUnitOfWork<'TContext when 'TContext :> DbContext>() = 
    inherit BaseUnitOfWork(Activator.CreateInstance<'TContext>())

and [<Sealed; AbstractClass>] GenericRepository() = 
    
    static member CreateGenericRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>(manager : BaseUnitOfWork) = 
        let baseType = typedefof<BaseRepository<'TEntity, 'TDTO>>
        let mutable assembly = Assembly.GetExecutingAssembly()
        
        let mutable repo = 
            (match baseType.IsGenericTypeDefinition with
             | true -> 
                 Activator.CreateInstance
                     (baseType.MakeGenericType(typedefof<'TEntity>, typedefof<'TDTO>), manager)
             | _ -> Activator.CreateInstance(baseType, manager)) :?> BaseRepository<'TEntity, 'TDTO>
        
        let mutable repoType : Type = null
        if (assembly <> null) then 
            try 
                repoType <- assembly.GetTypes() |> Seq.find (fun t -> t.BaseType = baseType)
                repo <- Activator.CreateInstance(repoType, manager) :?> BaseRepository<'TEntity, 'TDTO>
            with :? KeyNotFoundException -> assembly <- null
        if (assembly = null) then 
            assembly <- Assembly.GetEntryAssembly()
            if (assembly <> null) then 
                try 
                    repoType <- assembly.GetTypes() |> Seq.find (fun t -> t.BaseType = baseType)
                    repo <- Activator.CreateInstance(repoType, manager) :?> BaseRepository<'TEntity, 'TDTO>
                with :? KeyNotFoundException -> assembly <- null
        if (assembly = null) then 
            assembly <- Assembly.GetCallingAssembly()
            if (assembly <> null) then 
                try 
                    repoType <- assembly.GetTypes() |> Seq.find (fun t -> t.BaseType = baseType)
                    repo <- Activator.CreateInstance(repoType, manager) :?> BaseRepository<'TEntity, 'TDTO>
                with :? KeyNotFoundException -> assembly <- null
        repo
    
    static member CreateGenericCRUDRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>(manager : BaseUnitOfWork) = 
        let baseType = typedefof<BaseCRUDRepository<'TEntity, 'TDTO>>
        let mutable assembly = Assembly.GetExecutingAssembly()
        
        let mutable repo = 
            (match baseType.IsGenericTypeDefinition with
             | true -> 
                 Activator.CreateInstance
                     (baseType.MakeGenericType(typedefof<'TEntity>, typedefof<'TDTO>))
             | _ -> Activator.CreateInstance(baseType)) :?> BaseCRUDRepository<'TEntity, 'TDTO>
        
        let mutable repoType : Type = null
        if (assembly <> null) then 
            try 
                repoType <- assembly.GetTypes() |> Seq.find (fun t -> t.BaseType = baseType)
                repo <- Activator.CreateInstance(repoType, manager) :?> BaseCRUDRepository<'TEntity, 'TDTO>
            with :? KeyNotFoundException -> assembly <- null
        if (assembly = null) then 
            assembly <- Assembly.GetEntryAssembly()
            if (assembly <> null) then 
                try 
                    repoType <- assembly.GetTypes() |> Seq.find (fun t -> t.BaseType = baseType)
                    repo <- Activator.CreateInstance(repoType, manager) :?> BaseCRUDRepository<'TEntity, 'TDTO>
                with :? KeyNotFoundException -> assembly <- null
        if (assembly = null) then 
            assembly <- Assembly.GetCallingAssembly()
            if (assembly <> null) then 
                try 
                    repoType <- assembly.GetTypes() |> Seq.find (fun t -> t.BaseType = baseType)
                    repo <- Activator.CreateInstance(repoType, manager) :?> BaseCRUDRepository<'TEntity, 'TDTO>
                with :? KeyNotFoundException -> assembly <- null
        repo

and [<AllowNullLiteral>] BaseCRUDRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>(manager) = 
    inherit BaseRepository<'TEntity, 'TDTO>(manager)
    abstract Insert : 'TDTO -> unit
    override this.Insert(dto) = this.Set().Add(this.EntityBuilder().Build(dto)) |> ignore

    abstract Insert : 'TEntity -> unit
    override this.Insert(entity) = this.Set().Add(entity) |> ignore

    abstract Update : 'TDTO * obj[] -> unit
    
    override this.Update(dto : 'TDTO, [<ParamArray>] ids) = 
        let entity = this.Set().Find(ids)
        let newEntity : ref<'TEntity> = ref null
        this.EntityBuilder().Build(dto, newEntity)
        manager.Entry(entity).CurrentValues.SetValues(!newEntity)

    abstract Update : 'TEntity * obj[] -> unit

    override this.Update(entity : 'TEntity, [<ParamArray>] ids) =
        let oldEntity = this.Set().Find(ids)
        manager.Entry(oldEntity).CurrentValues.SetValues(entity)
    
    abstract Delete : 'TDTO * obj[] -> unit
    
    override this.Delete(dto, [<ParamArray>] ids) = 
        let entity = this.Set().Find(ids)
        this.Set().Remove(entity) |> ignore
    
    abstract Delete : obj[] -> unit
    override this.Delete([<ParamArray>] ids : obj[]) = this.Delete(this.Single(ids), ids)
    member this.InsertAsync(dto : 'TDTO) = async { this.Insert(dto) } |> Async.StartAsTask
    member this.UpdateAsync(dto : 'TDTO, ids) = async { this.Update(dto, ids) } |> Async.StartAsTask
    member this.InsertAsync(entity : 'TEntity) = async { this.Insert(entity) } |> Async.StartAsTask
    member this.UpdateAsync(entity : 'TEntity, ids) = async { this.Update(entity, ids) } |> Async.StartAsTask
    member this.DeleteAsync(dto : 'TDTO, [<ParamArray>] ids) = async { this.Delete(dto, ids) } |> Async.StartAsTask
    member this.DeleteAsync(ids : obj []) = async { this.Delete(ids) } |> Async.StartAsTask
