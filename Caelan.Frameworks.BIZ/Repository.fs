namespace Caelan.Frameworks.BIZ.Classes

open System
open System.Data.Entity
open System.Linq
open System.Linq.Expressions
open System.Reflection
open Caelan.DynamicLinq.Classes
open Caelan.DynamicLinq.Extensions
open Caelan.Frameworks.Common.Classes
open Caelan.Frameworks.BIZ.Interfaces

[<AbstractClass>]
[<AllowNullLiteral>]
type Repository(manager) = 
    
    interface IRepository with
        member this.GetUnitOfWork() = this.UnitOfWork
        member this.GetUnitOfWork<'T when 'T :> IUnitOfWork>() = this.UnitOfWork :?> 'T
    
    member private __.UnitOfWork : IUnitOfWork = manager
    member this.GetUnitOfWork() = (this :> IRepository).GetUnitOfWork()
    member this.GetUnitOfWork<'T when 'T :> IUnitOfWork>() = (this :> IRepository).GetUnitOfWork<'T>()

[<AllowNullLiteral>]
type Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>(manager) = 
    inherit Repository(manager : IUnitOfWork)
    
    interface IRepository<'TEntity, 'TDTO> with        
        member __.DTOBuilder() = Builder<'TEntity, 'TDTO>()
        member __.EntityBuilder() = Builder<'TDTO, 'TEntity>()
        member this.Set() = (this.GetUnitOfWork() :?> UnitOfWork).DbSet() :> DbSet<'TEntity>
        member this.Single([<ParamArray>] ids) = this.DTOBuilder().Build(this.Set().Find(ids))
        
        member this.Single(expr) = 
            this.DTOBuilder().Build(match expr with
                                    | null -> this.Set().FirstOrDefault()
                                    | _ -> this.Set().FirstOrDefault(expr))
        
        member this.List() = this.DTOBuilder().BuildList(this.All())
        member this.List whereExpr = this.DTOBuilder().BuildList(this.All(whereExpr))
        member this.All() = this.Set().AsQueryable()
        member this.All(take, skip, sort, filter, whereFunc) = 
            this.All(take, skip, sort, filter, whereFunc, this.DTOBuilder().BuildList)
        
        member this.All(whereExpr) = 
            match whereExpr with
            | null -> this.All()
            | _ -> this.Set().Where(whereExpr).AsQueryable()
        
        member this.Insert(dto : 'TDTO) = this.Set().Add(this.EntityBuilder().Build(dto)) |> ignore
        member this.Insert(entity : 'TEntity) = this.Set().Add(entity) |> ignore
        
        member this.Update(dto : 'TDTO, [<ParamArray>] ids) = 
            let entity = this.Set().Find(ids)
            let newEntity : ref<'TEntity> = ref null
            this.EntityBuilder().Build(dto, newEntity)
            (manager :?> UnitOfWork).Entry(entity).CurrentValues.SetValues(!newEntity)
        
        member this.Update(entity : 'TEntity, [<ParamArray>] ids) = 
            let oldEntity = this.Set().Find(ids)
            (manager :?> UnitOfWork).Entry(oldEntity).CurrentValues.SetValues(entity)
        
        member this.Delete(_ : 'TDTO, [<ParamArray>] ids) = 
            let entity = this.Set().Find(ids)
            this.Set().Remove(entity) |> ignore
        
        member this.Delete(_ : 'TEntity, [<ParamArray>] ids) = 
            let entity = this.Set().Find(ids)
            this.Set().Remove(entity) |> ignore
        
        member this.Delete([<ParamArray>] ids : obj []) = this.Delete(this.Single(ids), ids)
    
    member this.DTOBuilder() = (this :> IRepository<'TEntity, 'TDTO>).DTOBuilder()
    member this.EntityBuilder() = (this :> IRepository<'TEntity, 'TDTO>).EntityBuilder()
    member this.Set() = (this :> IRepository<'TEntity, 'TDTO>).Set()
    member this.Single([<ParamArray>] ids : obj []) = (this :> IRepository<'TEntity, 'TDTO>).Single(ids)
    member this.Single(expr : Expression<Func<'TEntity, bool>>) = (this :> IRepository<'TEntity, 'TDTO>).Single(expr)
    member this.List() = (this :> IRepository<'TEntity, 'TDTO>).List()
    member this.List whereExpr = (this :> IRepository<'TEntity, 'TDTO>).List(whereExpr)
    member this.All() = (this :> IRepository<'TEntity, 'TDTO>).All()
    member this.All(whereExpr) = (this :> IRepository<'TEntity, 'TDTO>).All(whereExpr)
    
    member private this.All(take : int, skip : int, sort : seq<Sort>, filter : Filter, 
                            whereFunc : Expression<Func<'TEntity, bool>>, buildFunc : seq<'TEntity> -> seq<'TDTO>) = 
        let queryResult = 
            (match query { 
                       for item in (typeof<'TEntity>).GetProperties(BindingFlags.Instance ||| BindingFlags.Public) 
                                   |> Seq.map (fun t -> t.Name) do
                           select item
                           headOrDefault
                   } with
             | null -> this.All(whereFunc)
             | defaultSort -> this.All(whereFunc).OrderBy(defaultSort)).ToDataSourceResult(take, skip, sort, filter)
        DataSourceResult<'TDTO>(Data = buildFunc (queryResult.Data), Total = queryResult.Total)

    member this.All(take, skip, sort, filter, whereFunc) = 
        (this :> IRepository<'TEntity, 'TDTO>).All(take, skip, sort, filter, whereFunc)
    member this.Insert(dto : 'TDTO) = (this :> IRepository<'TEntity, 'TDTO>).Insert(dto)
    member this.Insert(entity : 'TEntity) = (this :> IRepository<'TEntity, 'TDTO>).Insert(entity)
    member this.Update(dto : 'TDTO, [<ParamArray>] ids) = (this :> IRepository<'TEntity, 'TDTO>).Update(dto, ids)
    member this.Update(entity : 'TEntity, [<ParamArray>] ids) = 
        (this :> IRepository<'TEntity, 'TDTO>).Update(entity, ids)
    member this.Delete(dto : 'TDTO, [<ParamArray>] ids) = (this :> IRepository<'TEntity, 'TDTO>).Delete(dto, ids)
    member this.Delete(entity : 'TEntity, [<ParamArray>] ids) = 
        (this :> IRepository<'TEntity, 'TDTO>).Delete(entity, ids)
    member this.Delete([<ParamArray>] ids : obj []) = (this :> IRepository<'TEntity, 'TDTO>).Delete(ids)
    member this.InsertAsync(dto : 'TDTO) = async { this.Insert(dto) } |> Async.StartAsTask
    member this.UpdateAsync(dto : 'TDTO, ids) = async { this.Update(dto, ids) } |> Async.StartAsTask
    member this.InsertAsync(entity : 'TEntity) = async { this.Insert(entity) } |> Async.StartAsTask
    member this.UpdateAsync(entity : 'TEntity, ids) = async { this.Update(entity, ids) } |> Async.StartAsTask
    member this.DeleteAsync(dto : 'TDTO, [<ParamArray>] ids) = async { this.Delete(dto, ids) } |> Async.StartAsTask
    member this.DeleteAsync(entity : 'TEntity, [<ParamArray>] ids) = 
        async { this.Delete(entity, ids) } |> Async.StartAsTask
    member this.DeleteAsync(ids : obj []) = async { this.Delete(ids) } |> Async.StartAsTask
    member this.ListAsync(whereExpr : Expression<Func<'TEntity, bool>>) = 
        async { return this.List(whereExpr) } |> Async.StartAsTask
    member this.ListAsync() = async { return this.List() } |> Async.StartAsTask
    member this.AllAsync(take : int, skip : int, sort : seq<Sort>, filter : Filter, 
                         whereFunc : Expression<Func<'TEntity, bool>>) = 
        async { return this.All(take, skip, sort, filter, whereFunc) } |> Async.StartAsTask
    member this.SingleAsync([<ParamArray>] id : obj []) = async { return this.Single(id) } |> Async.StartAsTask
    member this.SingleAsync(expr : 'TEntity -> bool) = async { return this.Single(expr) } |> Async.StartAsTask
