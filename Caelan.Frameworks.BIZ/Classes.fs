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
open Caelan.Frameworks.DAL.Interfaces
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

and [<AllowNullLiteral>] BaseRepository<'TEntity, 'TDTO, 'TKey when 'TKey :> IEquatable<'TKey> and 'TEntity : not struct and 'TDTO :> IDTO<'TKey> and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TKey : equality>(manager) = 
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
        let queryResult = this.All(whereFunc).ToDataSourceResult(take, skip, sort, filter)
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
    member internal __.DbSet<'TEntity, 'TDTO, 'TKey when 'TKey :> IEquatable<'TKey> and 'TEntity : not struct and 'TDTO :> IDTO<'TKey> and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TKey : equality>(repository : BaseRepository<'TEntity, 'TDTO, 'TKey>) = 
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
    
    member this.Repository<'TEntity, 'TDTO, 'TKey when 'TKey :> IEquatable<'TKey> and 'TEntity : not struct and 'TDTO :> IDTO<'TKey> and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TKey : equality>() = 
        let repoType = typeof<BaseRepository<'TEntity, 'TDTO, 'TKey>>
        try 
            let repositoryProp = 
                this.GetType().GetProperties(BindingFlags.Instance) 
                |> Seq.find (fun t -> t.PropertyType.BaseType = repoType)
            repositoryProp.GetValue(this) :?> BaseRepository<'TEntity, 'TDTO, 'TKey>
        with :? KeyNotFoundException -> 
            (match repoType.IsGenericTypeDefinition with
             | true -> 
                 Activator.CreateInstance
                     (repoType.MakeGenericType(typedefof<'TEntity>, typedefof<'TDTO>, typedefof<'TKey>), this)
             | _ -> Activator.CreateInstance(repoType, this)) :?> BaseRepository<'TEntity, 'TDTO, 'TKey>
    
    member this.CRUDRepository<'TEntity, 'TDTO, 'TKey when 'TKey :> IEquatable<'TKey> and 'TEntity : not struct and 'TDTO :> IDTO<'TKey> and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TKey : equality>() = 
        let repoType = typeof<BaseCRUDRepository<'TEntity, 'TDTO, 'TKey>>
        try 
            let repositoryProp = 
                this.GetType().GetProperties(BindingFlags.Instance) 
                |> Seq.find (fun t -> t.PropertyType.BaseType = repoType)
            repositoryProp.GetValue(this) :?> BaseCRUDRepository<'TEntity, 'TDTO, 'TKey>
        with :? KeyNotFoundException -> 
            (match repoType.IsGenericTypeDefinition with
             | true -> 
                 Activator.CreateInstance
                     (repoType.MakeGenericType(typedefof<'TEntity>, typedefof<'TDTO>, typedefof<'TKey>), this)
             | _ -> Activator.CreateInstance(repoType, this)) :?> BaseCRUDRepository<'TEntity, 'TDTO, 'TKey>
    
    member __.Dispose() = ()
    interface IDisposable with
        member this.Dispose() = this.Dispose()

and [<AbstractClass>] BaseUnitOfWork<'TContext when 'TContext :> DbContext>() = 
    inherit BaseUnitOfWork(Activator.CreateInstance<'TContext>())

and [<Sealed; AbstractClass>] GenericRepository() = 
    
    static member CreateGenericRepository<'TEntity, 'TDTO, 'TKey when 'TKey :> IEquatable<'TKey> and 'TEntity : not struct and 'TDTO :> IDTO<'TKey> and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TKey : equality>(manager : BaseUnitOfWork) = 
        let baseType = typedefof<BaseRepository<'TEntity, 'TDTO, 'TKey>>
        let mutable assembly = Assembly.GetExecutingAssembly()
        
        let mutable repo = 
            (match baseType.IsGenericTypeDefinition with
             | true -> 
                 Activator.CreateInstance
                     (baseType.MakeGenericType(typedefof<'TEntity>, typedefof<'TDTO>, typedefof<'TKey>), manager)
             | _ -> Activator.CreateInstance(baseType, manager)) :?> BaseRepository<'TEntity, 'TDTO, 'TKey>
        
        let mutable repoType : Type = null
        if (assembly <> null) then 
            try 
                repoType <- assembly.GetTypes() |> Seq.find (fun t -> t.BaseType = baseType)
                repo <- Activator.CreateInstance(repoType, manager) :?> BaseRepository<'TEntity, 'TDTO, 'TKey>
            with :? KeyNotFoundException -> assembly <- null
        if (assembly = null) then 
            assembly <- Assembly.GetEntryAssembly()
            if (assembly <> null) then 
                try 
                    repoType <- assembly.GetTypes() |> Seq.find (fun t -> t.BaseType = baseType)
                    repo <- Activator.CreateInstance(repoType, manager) :?> BaseRepository<'TEntity, 'TDTO, 'TKey>
                with :? KeyNotFoundException -> assembly <- null
        if (assembly = null) then 
            assembly <- Assembly.GetCallingAssembly()
            if (assembly <> null) then 
                try 
                    repoType <- assembly.GetTypes() |> Seq.find (fun t -> t.BaseType = baseType)
                    repo <- Activator.CreateInstance(repoType, manager) :?> BaseRepository<'TEntity, 'TDTO, 'TKey>
                with :? KeyNotFoundException -> assembly <- null
        repo
    
    static member CreateGenericCRUDRepository<'TEntity, 'TDTO, 'TKey when 'TKey :> IEquatable<'TKey> and 'TEntity : not struct and 'TDTO :> IDTO<'TKey> and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TKey : equality>(manager : BaseUnitOfWork) = 
        let baseType = typedefof<BaseCRUDRepository<'TEntity, 'TDTO, 'TKey>>
        let mutable assembly = Assembly.GetExecutingAssembly()
        
        let mutable repo = 
            (match baseType.IsGenericTypeDefinition with
             | true -> 
                 Activator.CreateInstance
                     (baseType.MakeGenericType(typedefof<'TEntity>, typedefof<'TDTO>, typedefof<'TKey>))
             | _ -> Activator.CreateInstance(baseType)) :?> BaseCRUDRepository<'TEntity, 'TDTO, 'TKey>
        
        let mutable repoType : Type = null
        if (assembly <> null) then 
            try 
                repoType <- assembly.GetTypes() |> Seq.find (fun t -> t.BaseType = baseType)
                repo <- Activator.CreateInstance(repoType, manager) :?> BaseCRUDRepository<'TEntity, 'TDTO, 'TKey>
            with :? KeyNotFoundException -> assembly <- null
        if (assembly = null) then 
            assembly <- Assembly.GetEntryAssembly()
            if (assembly <> null) then 
                try 
                    repoType <- assembly.GetTypes() |> Seq.find (fun t -> t.BaseType = baseType)
                    repo <- Activator.CreateInstance(repoType, manager) :?> BaseCRUDRepository<'TEntity, 'TDTO, 'TKey>
                with :? KeyNotFoundException -> assembly <- null
        if (assembly = null) then 
            assembly <- Assembly.GetCallingAssembly()
            if (assembly <> null) then 
                try 
                    repoType <- assembly.GetTypes() |> Seq.find (fun t -> t.BaseType = baseType)
                    repo <- Activator.CreateInstance(repoType, manager) :?> BaseCRUDRepository<'TEntity, 'TDTO, 'TKey>
                with :? KeyNotFoundException -> assembly <- null
        repo

and [<AllowNullLiteral>] BaseCRUDRepository<'TEntity, 'TDTO, 'TKey when 'TKey :> IEquatable<'TKey> and 'TEntity : not struct and 'TDTO :> IDTO<'TKey> and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TKey : equality>(manager) = 
    inherit BaseRepository<'TEntity, 'TDTO, 'TKey>(manager)
    abstract Insert : 'TDTO -> unit
    override this.Insert(dto) = this.Set().Add(this.EntityBuilder().Build(dto)) |> ignore
    abstract Update : 'TDTO -> unit
    
    override this.Update(dto) = 
        let entity = this.Set().Find(dto.ID)
        let newEntity : ref<'TEntity> = ref null
        this.EntityBuilder().Build(dto, newEntity)
        manager.Entry(entity).CurrentValues.SetValues(!newEntity)
    
    abstract Delete : 'TDTO -> unit
    
    override this.Delete(dto : 'TDTO) = 
        let entity = this.Set().Find(dto.ID)
        this.Set().Remove(entity) |> ignore
    
    abstract Delete : obj [] -> unit
    override this.Delete([<ParamArray>] ids : obj []) = this.Delete(this.Single(ids))
    member this.InsertAsync(dto) = async { this.Insert(dto) } |> Async.StartAsTask
    member this.UpdateAsync(dto) = async { this.Update(dto) } |> Async.StartAsTask
    member this.DeleteAsync(dto : 'TDTO) = async { this.Delete(dto) } |> Async.StartAsTask
    member this.DeleteAsync(ids : obj []) = async { this.Delete(ids) } |> Async.StartAsTask
