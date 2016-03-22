namespace UnitOfWork.NET.EntityFramework.Classes

open Caelan.DynamicLinq.Classes
open Caelan.DynamicLinq.Extensions
open ClassBuilder.Classes
open ClassBuilder.Interfaces
open System
open System.Collections.Generic
open System.Data.Entity
open System.Linq
open System.Linq.Dynamic
open System.Linq.Expressions
open System.Reflection
open UnitOfWork.NET.Classes
open UnitOfWork.NET.Interfaces
open UnitOfWork.NET.EntityFramework.Interfaces

type EntityRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>(manager : IUnitOfWork) = 
    inherit EntityRepository<'TEntity>(manager)

    interface IEntityRepository<'TEntity, 'TDTO> with
        member this.DTO([<ParamArray>] ids : obj []) = ids |> this.DTO
        member this.DTO(expr : Expression<Func<'TEntity, bool>>) = expr |> this.DTO
        member this.ElementBuilt expr = Expression.Lambda<Func<'TEntity, bool>>(Expression.Call(expr.Method)) |> this.DTO
        member this.List() = this.List()
        member this.AllBuilt() = this.List()
        member this.AllBuilt whereExpr = Expression.Lambda<Func<'TEntity, bool>>(Expression.Call(whereExpr.Method)) |> this.List
        member this.List whereExpr = whereExpr |> this.List
        member this.DataSource(take, skip, sort, filter, whereFunc:Func<'TEntity, bool>) = this.DataSource(take, skip, sort, filter, Expression.Lambda<Func<'TEntity, bool>>(Expression.Call(whereFunc.Method)))
        member this.DataSource(take, skip, sort, filter, whereFunc:Expression<Func<'TEntity,bool>>) = this.DataSource(take, skip, sort, filter, whereFunc)
        member this.Insert(dto : 'TDTO) = dto |> this.Insert
        member this.Update(dto : 'TDTO, [<ParamArray>] ids) = this.Update(dto, ids)
    
    member val DTOMapper : IMapper<'TEntity, 'TDTO> option = None with get, set
    member val EntityMapper : IMapper<'TDTO, 'TEntity> option = None with get, set
    
    member this.DTO([<ParamArray>] ids : obj []) = 
        match this.Entity(ids) |> Option.ofObj with
        | None -> null
        | Some(entity) -> Builder.Build(entity).To<'TDTO>()
    
    member this.DTO(expr : Expression<Func<'TEntity, bool>>) = 
        match this.Entity(expr) |> Option.ofObj with
        | None -> null
        | Some(entity) -> Builder.Build(entity).To<'TDTO>()
    
    member this.List() = Builder.BuildList(this.All()).ToList<'TDTO>()
    member this.List(whereExpr:Expression<Func<'TEntity,bool>>) = Builder.BuildList(this.All(whereExpr)).ToList<'TDTO>()
    
    member this.DataSource(take, skip, sort, filter, whereFunc) = this.DataSource(take, skip, sort, filter, whereFunc, fun t -> Builder.BuildList(t).ToList<'TDTO>())
    
    member private this.DataSource(take, skip, sort, filter, whereFunc:Expression<Func<'TEntity,bool>>, buildFunc : seq<'TEntity> -> seq<'TDTO>) = 
        let orderBy = 
            query { 
                for item in (typeof<'TEntity>).GetProperties(BindingFlags.Instance ||| BindingFlags.Public) do
                    select item.Name
                    headOrDefault
            }
        
        let queryResult = 
            let res = 
                match orderBy |> Option.ofObj with
                | None -> this.All(whereFunc)
                | Some(defaultSort) -> this.All(whereFunc).OrderBy(defaultSort)
            res.AsQueryable().ToDataSourceResult(take, skip, sort, filter)
        
        DataSourceResult<'TDTO>(Data = (buildFunc (queryResult.Data)).ToList(), Total = queryResult.Total)
    
    abstract Insert : dto:'TDTO -> 'TDTO
    abstract Update : 'TDTO * [<ParamArray>] ids:obj [] -> unit
    override this.Insert(dto : 'TDTO) = Builder.Build(Builder.Build(dto).To<'TEntity>() |> this.Insert).To<'TDTO>()
    
    override this.Update(dto : 'TDTO, [<ParamArray>] ids : obj []) = 
        let entity = this.Entity(ids)
        Builder.Build(dto).To(entity) |> ignore
        this.Update(entity, ids)
    
    member this.InsertAsync(dto : 'TDTO) = async { return this.Insert(dto) } |> Async.StartAsTask
    member this.UpdateAsync(dto : 'TDTO, ids) = async { this.Update(dto, ids) } |> Async.StartAsTask
    member this.DeleteAsync(dto : 'TDTO, [<ParamArray>] ids) = async { this.Delete(dto, ids) } |> Async.StartAsTask
    member this.ListAsync(whereExpr : Expression<Func<'TEntity, bool>>) = async { return this.List(whereExpr) } |> Async.StartAsTask
    member this.ListAsync() = async { return this.List() } |> Async.StartAsTask
    member this.DataSourceAsync(take : int, skip : int, sort : ICollection<Sort>, filter : Filter, whereFunc : Expression<Func<'TEntity, bool>>) = async { return this.DataSource(take, skip, sort, filter, whereFunc) } |> Async.StartAsTask
    member this.SingleDTOAsync([<ParamArray>] id : obj []) = async { return this.DTO(id) } |> Async.StartAsTask
    member this.SingleDTOAsync(expr : Expression<Func<'TEntity, bool>>) = async { return this.DTO(expr) } |> Async.StartAsTask