namespace Caelan.Frameworks.BIZ.Classes

open System
open System.Data.Linq
open System.Linq
open System.Linq.Expressions
open System.Reflection
open Caelan.DynamicLinq.Classes
open Caelan.DynamicLinq.Extensions
open Caelan.Frameworks.Common.Interfaces
open Caelan.Frameworks.Common.Classes
open Caelan.Frameworks.BIZ.Interfaces

type Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>(manager) = 
    inherit Repository<'TEntity>(manager : IUnitOfWork)
    
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
    
    member val DTOMapper : IMapper<'TEntity, 'TDTO> option = None with get, set
    member val EntityMapper : IMapper<'TDTO, 'TEntity> option = None with get, set
    member this.DTOBuilder() = this.DTOMapper |> this.DTOBuilder
    member this.EntityBuilder() = this.EntityMapper |> this.EntityBuilder
    
    member __.DTOBuilder mapper = 
        match mapper with
        | None -> Builder.Source<'TEntity>().Destination<'TDTO>()
        | Some(m) -> Builder.Source<'TEntity>().Destination<'TDTO> m
    
    member __.EntityBuilder mapper = 
        match mapper with
        | None -> Builder.Source<'TDTO>().Destination<'TEntity>()
        | Some(m) -> Builder.Source<'TDTO>().Destination<'TEntity> m
    
    member this.SingleDTO([<ParamArray>] ids : obj []) = 
        match this.SingleEntity(ids) with
        | null -> null
        | entity -> entity |> this.DTOBuilder().Build
    
    member this.SingleDTO(expr : Expression<Func<'TEntity, bool>>) = 
        match this.SingleEntity(expr) with
        | null -> null
        | entity -> entity |> this.DTOBuilder().Build
    
    member this.List() = this.DTOBuilder().BuildList(this.All() :> seq<'TEntity>)
    member this.List(whereExpr) = this.DTOBuilder().BuildList(this.All(whereExpr) :> seq<'TEntity>)
    member this.All(take, skip, sort, filter, whereFunc) = 
        this.All(take, skip, sort, filter, whereFunc, this.DTOBuilder().BuildList)
    
    member private this.All(take, skip, sort, filter, whereFunc, buildFunc : seq<'TEntity> -> seq<'TDTO>) = 
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
        
        DataSourceResult<_>(Data = buildFunc (queryResult.Data), Total = queryResult.Total)
    
    abstract Insert : dto:'TDTO -> 'TDTO
    abstract Update : 'TDTO * ids:obj [] -> unit
    abstract Delete : 'TDTO * ids:obj [] -> unit
    override this.Insert(dto : 'TDTO) = this.DTOBuilder().Build(this.Set().Add(this.EntityBuilder().Build(dto)))
    
    override this.Update(dto : 'TDTO, [<ParamArray>] ids) = 
        let entity = this.Set().Find(ids)
        let entry = manager.Entry(entity)
        this.EntityBuilder().Build(dto, ref entity)
        entry.CurrentValues.SetValues(entity)
    
    override this.Delete(_ : 'TDTO, [<ParamArray>] ids) = this.Delete(ids) |> ignore
    member this.InsertAsync(dto : 'TDTO) = async { return this.Insert(dto) } |> Async.StartAsTask
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