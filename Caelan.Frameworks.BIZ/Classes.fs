namespace Caelan.Frameworks.BIZ.Classes

open System
open System.Data.Entity
open System.Runtime.CompilerServices
open System.Collections.Generic
open System.Linq
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
    override this.Set() = this.GetUnitOfWork().DbSet()
    abstract List : unit -> IEnumerable<'TDTO>
    override this.List() = this.DTOBuilder().BuildList(this.All())
    abstract List : whereExpr:('TEntity -> bool) option -> IEnumerable<'TDTO>
    override this.List(whereExpr) = this.DTOBuilder().BuildList(this.All(whereExpr))
    abstract All : unit -> IQueryable<'TEntity>
    override this.All() = this.Set() :> IQueryable<'TEntity>
    abstract All : whereExpr:('TEntity -> bool) option -> IQueryable<'TEntity>
    
    override this.All(whereExpr) = 
        match whereExpr with
        | None -> this.All()
        | Some(expr) -> this.Set().Where(expr)
    
    member private this.All(take : int, skip : int, sort : seq<Sort>, filter : Filter, 
                            whereFunc : ('TEntity -> bool) option, buildFunc : seq<'TEntity> -> seq<'TDTO>) = 
        let queryResult = 
            (match query { 
                       for item in (typedefof<'TEntity>).GetProperties(BindingFlags.Instance ||| BindingFlags.Public) 
                                   |> Seq.map (fun t -> t.Name) do
                           select item
                           headOrDefault
                   } with
             | null -> this.All(whereFunc)
             | defaultSort -> this.All(whereFunc).OrderBy(defaultSort)).ToDataSourceResult(take, skip, sort, filter)
        DataSourceResult<'TDTO>(Data = buildFunc (queryResult.Data), Total = queryResult.Total)
    
    abstract All : int * int * seq<Sort> * Filter * ('TEntity -> bool) option -> DataSourceResult<'TDTO>
    override this.All(take, skip, sort, filter, whereFunc) = 
        this.All(take, skip, sort, filter, whereFunc, this.DTOBuilder().BuildList)
    abstract AllFull : int * int * seq<Sort> * Filter * ('TEntity -> bool) option -> DataSourceResult<'TDTO>
    override this.AllFull(take, skip, sort, filter, whereFunc) = 
        this.All(take, skip, sort, filter, whereFunc, this.DTOBuilder().BuildFullList)
    abstract DTOBuilder : unit -> BaseDTOBuilder<'TEntity, 'TDTO>
    override __.DTOBuilder() = GenericBusinessBuilder.GenericDTOBuilder<'TEntity, 'TDTO>()
    abstract EntityBuilder : unit -> BaseEntityBuilder<'TDTO, 'TEntity>
    override __.EntityBuilder() = GenericBusinessBuilder.GenericEntityBuilder<'TDTO, 'TEntity>()
    abstract Single : obj [] -> 'TDTO
    override this.Single([<ParamArray>] ids) = this.DTOBuilder().BuildFull(this.Set().Find(ids))
    
    member this.Single(expr : Func<'TEntity, bool>) = 
        this.Single(match expr with
                    | null -> None
                    | _ -> Some(fun t -> expr.Invoke(t)))
    
    abstract Single : ('TEntity -> bool) option -> 'TDTO
    
    override this.Single(expr) = 
        this.DTOBuilder().BuildFull(match expr with
                                    | Some(checkExpr) -> 
                                        match this.Set() |> Seq.tryFind checkExpr with
                                        | Some(entity) -> entity
                                        | None -> null
                                    | None -> 
                                        query { 
                                            for item in this.Set() do
                                                select item
                                                headOrDefault
                                        })
    
    member this.ListAsync(whereExpr) = async { return this.List(whereExpr) } |> Async.StartAsTask
    member this.ListAsync() = async { return this.List() } |> Async.StartAsTask
    member this.AllAsync(take, skip, sort, filter, whereFunc) = 
        async { return this.All(take, skip, sort, filter, whereFunc) } |> Async.StartAsTask
    member this.SingleAsync([<ParamArray>] id : obj []) = async { return this.Single(id) } |> Async.StartAsTask
    member this.SingleAsync(expr : ('TEntity -> bool) option) = async { return this.Single(expr) } |> Async.StartAsTask
    member this.SingleAsync(expr : Func<'TEntity, bool>) = async { return this.Single(expr) } |> Async.StartAsTask

and [<AbstractClass>] BaseUnitOfWork internal (context : DbContext) = 
    member internal __.DbSet<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>() = 
        context.Set<'TEntity>()
    member __.SaveChanges() = context.SaveChanges()
    member __.SaveChangesAsync() = async { return! context.SaveChangesAsync() |> Async.AwaitTask } |> Async.StartAsTask
    member __.Entry<'TEntity>(entity : 'TEntity) = context.Entry(entity)
    
    member this.Repository<'TRepository when 'TRepository :> BaseRepository>() = 
        (match this.GetType().GetProperties(BindingFlags.Instance) 
               |> Seq.tryFind (fun t -> t.PropertyType = typedefof<'TRepository>) with
         | Some(repositoryProp) -> repositoryProp.GetValue(this)
         | None -> Activator.CreateInstance(typedefof<'TRepository>, this)) :?> 'TRepository
    
    member this.Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>() = 
        this.Repository<BaseRepository<'TEntity, 'TDTO>>()
    member this.CRUDRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>() = 
        this.Repository<BaseCRUDRepository<'TEntity, 'TDTO>>()
    member __.Dispose() = ()
    interface IDisposable with
        member this.Dispose() = this.Dispose()

and [<AbstractClass>] BaseUnitOfWork<'TContext when 'TContext :> DbContext>() = 
    inherit BaseUnitOfWork(Activator.CreateInstance<'TContext>())

and [<Sealed; AbstractClass>] GenericRepository() = 
    
    static member private FindRepositoryInAssembly<'TEntity, 'TDTO, 'TRepository when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TRepository :> BaseRepository<'TEntity, 'TDTO>> (manager : BaseUnitOfWork) 
                  (baseType : Type) (assembly : Assembly) = 
        if (assembly <> null) then 
            try 
                let repoType = assembly.GetTypes() |> Seq.find (fun t -> t.BaseType = baseType)
                let repo = Activator.CreateInstance(repoType, manager) :?> 'TRepository
                Some(repo)
            with :? KeyNotFoundException -> None
        else None
    
    static member private CreateRepository<'TEntity, 'TDTO, 'TRepository when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TRepository :> BaseRepository<'TEntity, 'TDTO>>(manager : BaseUnitOfWork) = 
        let baseType = typedefof<'TRepository>
        match Assembly.GetExecutingAssembly() 
              |> GenericRepository.FindRepositoryInAssembly<'TEntity, 'TDTO, 'TRepository> manager baseType with
        | Some(repo) -> repo
        | None -> 
            match Assembly.GetEntryAssembly() 
                  |> GenericRepository.FindRepositoryInAssembly<'TEntity, 'TDTO, 'TRepository> manager baseType with
            | Some(repo) -> repo
            | None -> 
                match Assembly.GetCallingAssembly() 
                      |> GenericRepository.FindRepositoryInAssembly<'TEntity, 'TDTO, 'TRepository> manager baseType with
                | Some(repo) -> repo
                | None -> 
                    (match baseType.IsGenericTypeDefinition with
                     | true -> Activator.CreateInstance(baseType.MakeGenericType(typedefof<'TEntity>, typedefof<'TDTO>))
                     | _ -> Activator.CreateInstance(baseType, manager)) :?> 'TRepository
    
    static member CreateGenericRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>(manager : BaseUnitOfWork) = 
        GenericRepository.CreateRepository<'TEntity, 'TDTO, BaseRepository<'TEntity, 'TDTO>>(manager)
    static member CreateGenericCRUDRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>(manager : BaseUnitOfWork) = 
        GenericRepository.CreateRepository<'TEntity, 'TDTO, BaseCRUDRepository<'TEntity, 'TDTO>>(manager)

and [<AllowNullLiteral>] BaseCRUDRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>(manager) = 
    inherit BaseRepository<'TEntity, 'TDTO>(manager)
    abstract Insert : 'TDTO -> unit
    override this.Insert(dto) = this.Set().Add(this.EntityBuilder().Build(dto)) |> ignore
    abstract Insert : 'TEntity -> unit
    override this.Insert(entity) = this.Set().Add(entity) |> ignore
    abstract Update : 'TDTO * obj [] -> unit
    
    override this.Update(dto : 'TDTO, [<ParamArray>] ids) = 
        let entity = this.Set().Find(ids)
        let newEntity : ref<'TEntity> = ref null
        this.EntityBuilder().Build(dto, newEntity)
        manager.Entry(entity).CurrentValues.SetValues(!newEntity)
    
    abstract Update : 'TEntity * obj [] -> unit
    
    override this.Update(entity : 'TEntity, [<ParamArray>] ids) = 
        let oldEntity = this.Set().Find(ids)
        manager.Entry(oldEntity).CurrentValues.SetValues(entity)
    
    abstract Delete : 'TDTO * obj [] -> unit
    
    override this.Delete(_ : 'TDTO, [<ParamArray>] ids) = 
        let entity = this.Set().Find(ids)
        this.Set().Remove(entity) |> ignore
    
    abstract Delete : 'TEntity * obj [] -> unit
    
    override this.Delete(_ : 'TEntity, [<ParamArray>] ids) = 
        let entity = this.Set().Find(ids)
        this.Set().Remove(entity) |> ignore
    
    abstract Delete : obj [] -> unit
    override this.Delete([<ParamArray>] ids : obj []) = this.Delete(this.Single(ids), ids)
    member this.InsertAsync(dto : 'TDTO) = async { this.Insert(dto) } |> Async.StartAsTask
    member this.UpdateAsync(dto : 'TDTO, ids) = async { this.Update(dto, ids) } |> Async.StartAsTask
    member this.InsertAsync(entity : 'TEntity) = async { this.Insert(entity) } |> Async.StartAsTask
    member this.UpdateAsync(entity : 'TEntity, ids) = async { this.Update(entity, ids) } |> Async.StartAsTask
    member this.DeleteAsync(dto : 'TDTO, [<ParamArray>] ids) = async { this.Delete(dto, ids) } |> Async.StartAsTask
    member this.DeleteAsync(entity : 'TEntity, [<ParamArray>] ids) = 
        async { this.Delete(entity, ids) } |> Async.StartAsTask
    member this.DeleteAsync(ids : obj []) = async { this.Delete(ids) } |> Async.StartAsTask
