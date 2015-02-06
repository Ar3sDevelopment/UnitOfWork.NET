namespace Caelan.Frameworks.BIZ.Classes

open System
open System.Collections.Generic
open System.Linq
open System.Linq.Expressions
open System.Reflection
open Caelan.DynamicLinq.Classes
open Caelan.DynamicLinq.Extensions
open Caelan.Frameworks.Common.Classes
open Caelan.Frameworks.Common.Interfaces
open Caelan.Frameworks.BIZ.Interfaces

[<AbstractClass>]
[<AllowNullLiteral>]
type Repository(manager) = 
    
    interface IRepository with
        member this.GetUnitOfWork() = this.GetUnitOfWork()
        member this.GetUnitOfWork<'T when 'T :> IUnitOfWork>() = this.GetUnitOfWork<'T>()
    
    member private __.UnitOfWork : IUnitOfWork = manager
    member this.GetUnitOfWork() = this.UnitOfWork
    member this.GetUnitOfWork<'T when 'T :> IUnitOfWork>() = this.UnitOfWork :?> 'T
    static member Entity<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>(manager : IUnitOfWork) = 
        Repository<'TEntity>(manager)

and [<AllowNullLiteral>] Repository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>(manager : IUnitOfWork) = 
    inherit Repository(manager : IUnitOfWork)
    member this.DTO<'TDTO when 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>() = 
        Repository<'TEntity, 'TDTO>(manager)
    
    interface IRepository<'TEntity> with
        member this.Set() = this.Set()
        member this.SingleEntity([<ParamArray>] ids : obj []) = this.SingleEntity(ids)
        member this.SingleEntity(expr : Expression<Func<'TEntity, bool>>) = this.SingleEntity(expr)
        member this.All() = this.All()
        member this.All(whereExpr) = this.All(whereExpr)
        member this.Insert(entity : 'TEntity) = this.Insert(entity)
        member this.Update(entity : 'TEntity, [<ParamArray>] ids) = this.Update(entity, ids)
        member this.Delete(entity : 'TEntity, [<ParamArray>] ids) = this.Delete(entity, ids)
        member this.Delete([<ParamArray>] ids : obj []) = this.Delete(ids)
    
    member __.Set() = manager.DbSet<'TEntity>()
    member this.SingleEntity([<ParamArray>] ids) = this.Set().Find(ids)
    
    member this.SingleEntity(expr) = 
        match expr with
        | null -> this.Set().FirstOrDefault()
        | _ -> this.Set().FirstOrDefault(expr)
    
    member this.All() = this.Set().AsQueryable()
    
    member this.All(whereExpr : Expression<Func<'TEntity, bool>>) = 
        match whereExpr with
        | null -> this.All()
        | _ -> this.Set().Where(whereExpr).AsQueryable()
    
    abstract Insert : entity:'TEntity -> unit
    abstract Update : 'TEntity * ids:obj [] -> unit
    abstract Delete : 'TEntity * ids:obj [] -> unit
    abstract Delete : ids:obj [] -> unit
    override this.Insert(entity : 'TEntity) = this.Set().Add(entity) |> ignore
    override this.Update(entity : 'TEntity, [<ParamArray>] ids) = 
        manager.Entry(this.Set().Find(ids)).CurrentValues.SetValues(entity)
    override this.Delete(_ : 'TEntity, [<ParamArray>] ids) = this.Delete(ids) |> ignore
    override this.Delete([<ParamArray>] ids : obj []) = this.Set().Remove(this.Set().Find(ids)) |> ignore
    member this.InsertAsync(entity : 'TEntity) = async { this.Insert(entity) } |> Async.StartAsTask
    member this.UpdateAsync(entity : 'TEntity, ids) = async { this.Update(entity, ids) } |> Async.StartAsTask
    member this.DeleteAsync(entity : 'TEntity, [<ParamArray>] ids) = 
        async { this.Delete(entity, ids) } |> Async.StartAsTask
    member this.DeleteAsync(ids : obj []) = async { this.Delete(ids) } |> Async.StartAsTask
    member this.SingleEntityAsync([<ParamArray>] id : obj []) = 
        async { return this.SingleEntity(id) } |> Async.StartAsTask
    member this.SingleEntityAsync(expr : 'TEntity -> bool) = 
        async { return this.SingleEntity(expr) } |> Async.StartAsTask

and [<AllowNullLiteral>] Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>(manager) = 
    inherit Repository<'TEntity>(manager : IUnitOfWork)
    
    let memoize f = 
        let dict = new Dictionary<_, _>()
        fun n -> 
            match dict.TryGetValue(n) with
            | (true, v) -> v
            | _ -> 
                let temp = f (n)
                dict.Add(n, temp)
                temp
    
    interface IRepository<'TEntity, 'TDTO> with
        member this.DTOBuilder(mapper) = mapper |> this.DTOBuilder
        member this.EntityBuilder(mapper) = mapper |> this.EntityBuilder
        member this.DTOBuilder() = this.DTOBuilder()
        member this.EntityBuilder() = this.EntityBuilder()
        member this.SingleDTO([<ParamArray>] ids : obj []) = ids |> this.SingleDTO
        member this.SingleDTO(expr : Expression<Func<'TEntity, bool>>) = expr |> this.SingleDTO
        member this.List() = this.List()
        member this.List whereExpr = whereExpr |> this.List
        member this.All(take, skip, sort, filter, whereFunc) = this.All(take, skip, sort, filter, whereFunc)
        member this.Insert(dto : 'TDTO) = dto |> this.Insert
        member this.Update(dto : 'TDTO, [<ParamArray>] ids) = this.Update(dto, ids)
        member this.Delete(dto : 'TDTO, [<ParamArray>] ids) = this.Delete(dto, ids)
    
    member val DTOMapper : IMapper<'TEntity, 'TDTO> = null with get, set
    member val EntityMapper : IMapper<'TDTO, 'TEntity> = null with get, set
    member this.DTOBuilder() = this.DTOMapper |> this.DTOBuilder
    member this.EntityBuilder() = this.EntityMapper |> this.EntityBuilder
    
    member __.DTOBuilder mapper = 
        match mapper with
        | null -> Builder.Source<'TEntity>().Destination<'TDTO>()
        | _ -> Builder.Source<'TEntity>().Destination<'TDTO> mapper
    
    member __.EntityBuilder mapper = 
        match mapper with
        | null -> Builder.Source<'TDTO>().Destination<'TEntity>()
        | _ -> Builder.Source<'TDTO>().Destination<'TEntity> mapper
    
    member this.SingleDTO([<ParamArray>] ids : obj []) = 
        match this.SingleEntity(ids) with
        | null -> null
        | entity -> entity |> this.DTOBuilder().Build
    
    member this.SingleDTO(expr : Expression<Func<'TEntity, bool>>) = 
        match this.SingleEntity(expr) with
        | null -> null
        | entity -> entity |> this.DTOBuilder().Build
    
    member this.List() = this.DTOBuilder().BuildList(this.All() :> IEnumerable<'TEntity>)
    member this.List(whereExpr) = this.DTOBuilder().BuildList(this.All(whereExpr) :> IEnumerable<'TEntity>)
    member this.All(take, skip, sort, filter, whereFunc) = 
        this.All(take, skip, sort, filter, whereFunc, this.DTOBuilder().BuildList)
    
    member private this.All(take : int, skip : int, sort : seq<Sort>, filter : Filter, 
                            whereFunc : Expression<Func<'TEntity, bool>>, buildFunc : seq<'TEntity> -> seq<'TDTO>) = 
        let orderBy = 
            query { 
                for item in (typeof<'TEntity>).GetProperties(BindingFlags.Instance ||| BindingFlags.Public) 
                            |> Seq.map (fun t -> t.Name) do
                    select item
                    headOrDefault
            }
        
        let queryResult = 
            (match orderBy with
             | null -> this.All(whereFunc)
             | defaultSort -> this.All(whereFunc).OrderBy(defaultSort)).ToDataSourceResult(take, skip, sort, filter)
        
        DataSourceResult<'TDTO>(Data = buildFunc (queryResult.Data), Total = queryResult.Total)
    
    abstract Insert : dto:'TDTO -> unit
    abstract Update : 'TDTO * ids:obj [] -> unit
    abstract Delete : 'TDTO * ids:obj [] -> unit
    override this.Insert(dto : 'TDTO) = this.Set().Add(this.EntityBuilder().Build(dto)) |> ignore
    
    override this.Update(dto : 'TDTO, [<ParamArray>] ids) = 
        let entity = this.Set().Find(ids)
        let entry = manager.Entry(entity)
        this.EntityBuilder().Build(dto, ref entity)
        entry.CurrentValues.SetValues(entity)
    
    override this.Delete(_ : 'TDTO, [<ParamArray>] ids) = this.Delete(ids) |> ignore
    member this.InsertAsync(dto : 'TDTO) = async { this.Insert(dto) } |> Async.StartAsTask
    member this.UpdateAsync(dto : 'TDTO, ids) = async { this.Update(dto, ids) } |> Async.StartAsTask
    member this.DeleteAsync(dto : 'TDTO, [<ParamArray>] ids) = async { this.Delete(dto, ids) } |> Async.StartAsTask
    member this.ListAsync(whereExpr : Expression<Func<'TEntity, bool>>) = 
        async { return this.List(whereExpr) } |> Async.StartAsTask
    member this.ListAsync() = async { return this.List() } |> Async.StartAsTask
    member this.AllAsync(take : int, skip : int, sort : seq<Sort>, filter : Filter, 
                         whereFunc : Expression<Func<'TEntity, bool>>) = 
        async { return this.All(take, skip, sort, filter, whereFunc) } |> Async.StartAsTask
    member this.SingleDTOAsync([<ParamArray>] id : obj []) = async { return this.SingleDTO(id) } |> Async.StartAsTask
    member this.SingleDTOAsync(expr : 'TEntity -> bool) = async { return this.SingleDTO(expr) } |> Async.StartAsTask
