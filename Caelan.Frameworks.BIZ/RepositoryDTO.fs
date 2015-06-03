namespace Caelan.Frameworks.BIZ.Classes

open System
open System.Data.Linq
open System.Data.Entity
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
        member this.SingleDTO([<ParamArray>] ids : obj []) = ids |> this.SingleDTO
        member this.SingleDTO(expr : Expression<Func<'TEntity, bool>>) = expr |> this.SingleDTO
        member this.List() = this.List()
        member this.List whereExpr = whereExpr |> this.List
        member this.All(take, skip, sort, filter, whereFunc) = this.All(take, skip, sort, filter, whereFunc)
        member this.Insert(dto : 'TDTO) = dto |> this.Insert
        member this.Update(dto : 'TDTO, [<ParamArray>] ids) = this.Update(dto, ids)
    
    member val DTOMapper : IMapper<'TEntity, 'TDTO> option = None with get, set
    member val EntityMapper : IMapper<'TDTO, 'TEntity> option = None with get, set
    
    member this.SingleDTO([<ParamArray>] ids : obj []) = 
        match this.SingleEntity(ids) with
        | null -> null
        | entity -> Builder.Build(entity).To<'TDTO>()
    
    member this.SingleDTO(expr : Expression<Func<'TEntity, bool>>) = 
        match this.SingleEntity(expr) with
        | null -> null
        | entity -> Builder.Build(entity).To<'TDTO>()
    
    member this.List() = Builder.BuildList(this.All() :> seq<'TEntity>).ToList<'TDTO>()
    member this.List(whereExpr) = Builder.BuildList(this.All(whereExpr) :> seq<'TEntity>).ToList<'TDTO>()
    member this.All(take, skip, sort, filter, whereFunc) = this.All(take, skip, sort, filter, whereFunc, fun t -> Builder.BuildList(t).ToList<'TDTO>())
    
    member private this.All(take, skip, sort, filter, whereFunc, buildFunc : seq<'TEntity> -> seq<'TDTO>) = 
        let orderBy = 
            query { 
                for item in (typeof<'TEntity>).GetProperties(BindingFlags.Instance ||| BindingFlags.Public) do
                    select item.Name
                    headOrDefault
            }
        
        let queryResult = 
            (match orderBy with
             | null -> this.All(whereFunc)
             | defaultSort -> this.All(whereFunc).OrderBy(defaultSort)).ToDataSourceResult(take, skip, sort, filter)
        
        DataSourceResult<'TDTO>(Data = buildFunc (queryResult.Data), Total = queryResult.Total)
    
    abstract Insert : dto:'TDTO -> 'TDTO
    abstract Update : 'TDTO * [<ParamArray>]ids:obj [] -> unit
    override this.Insert(dto : 'TDTO) = Builder.Build(Builder.Build(dto).To<'TEntity>() |> this.Insert).To<'TDTO>()
    
    override this.Update(dto : 'TDTO, [<ParamArray>] ids:obj[]) = 
        let entity = this.SingleEntity(ids)
        Builder.Build(dto).To(entity) |> ignore
        this.Update(entity, ids)

    member this.InsertAsync(dto : 'TDTO) = async { return this.Insert(dto) } |> Async.StartAsTask
    member this.UpdateAsync(dto : 'TDTO, ids) = async { this.Update(dto, ids) } |> Async.StartAsTask
    member this.DeleteAsync(dto : 'TDTO, [<ParamArray>] ids) = async { this.Delete(dto, ids) } |> Async.StartAsTask
    member this.ListAsync(whereExpr : Expression<Func<'TEntity, bool>>) = async { return this.List(whereExpr) } |> Async.StartAsTask
    member this.ListAsync() = async { return this.List() } |> Async.StartAsTask
    member this.AllAsync(take : int, skip : int, sort : seq<Sort>, filter : Filter, whereFunc : Expression<Func<'TEntity, bool>>) = async { return this.All(take, skip, sort, filter, whereFunc) } |> Async.StartAsTask
    member this.SingleDTOAsync([<ParamArray>] id : obj []) = async { return this.SingleDTO(id) } |> Async.StartAsTask
    member this.SingleDTOAsync(expr : Expression<Func<'TEntity, bool>>) = async { return this.SingleDTO(expr) } |> Async.StartAsTask