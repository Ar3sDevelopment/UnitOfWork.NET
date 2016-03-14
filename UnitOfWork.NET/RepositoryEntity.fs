namespace UnitOfWork.NET.Classes

open System
open System.Collections.Generic
open System.Data.Entity
open System.Linq
open System.Linq.Expressions
open UnitOfWork.NET.Interfaces

type Repository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>(manager) = 
    inherit Repository(manager)
    
    interface IRepository<'TEntity> with
        member this.Set = this.Set
        member this.Entity([<ParamArray>] ids : obj []) = this.Entity ids
        member this.Entity(expr : Expression<Func<'TEntity, bool>>) = this.Entity expr
        member this.SingleEntity([<ParamArray>] ids : obj []) = this.Entity ids
        member this.SingleEntity(expr : Expression<Func<'TEntity, bool>>) = this.Entity expr
        member this.All() = this.All()
        member this.All whereExpr = whereExpr |> this.All
        member this.Insert(entity : 'TEntity) = entity |> this.Insert
        member this.Update(entity : 'TEntity, [<ParamArray>] ids) = this.Update(entity, ids)
        member this.Delete([<ParamArray>] ids : obj []) = this.Delete ids
        member this.Exists expr = expr |> this.Exists
        member this.Count expr = expr |> this.Count
        member this.OnSaveChanges(entities : IDictionary<EntityState, IEnumerable<'TEntity>>) = this.OnSaveChanges entities
    
    member __.Set = manager.DbSet<'TEntity>()
    member this.Entity([<ParamArray>] ids) = this.Set.Find ids
    member this.Entity expr = expr |> this.Set.FirstOrDefault
    
    [<Obsolete("Use Entity instead")>]
    member this.SingleEntity([<ParamArray>] ids : obj []) = this.Entity ids
    
    [<Obsolete("Use Entity instead")>]
    member this.SingleEntity(expr : Expression<Func<'TEntity, bool>>) = this.Entity expr
    
    member this.All() = this.Set.AsQueryable()
    member this.All(whereExpr : Expression<Func<'TEntity, bool>>) = this.Set.Where(whereExpr).AsQueryable()
    member this.Exists expr = this.Set.Any expr
    member this.Count expr = this.Set.Count expr
    abstract OnSaveChanges : entities:IDictionary<EntityState, IEnumerable<'TEntity>> -> unit
    override this.OnSaveChanges(entities : IDictionary<EntityState, IEnumerable<'TEntity>>) = ()
    abstract Insert : entity:'TEntity -> 'TEntity
    abstract Update : 'TEntity * [<ParamArray>] ids:obj [] -> unit
    abstract Delete : [<ParamArray>] ids:obj [] -> unit
    override this.Insert entity = entity |> this.Set.Add
    override this.Update(entity, [<ParamArray>] ids) = manager.Entry(ids |> this.Entity).CurrentValues.SetValues(entity)
    
    override this.Delete([<ParamArray>] ids) = 
        ids
        |> this.Entity
        |> this.Set.Remove
        |> ignore
    
    member this.InsertAsync entity = async { return this.Insert(entity) } |> Async.StartAsTask
    member this.UpdateAsync(entity, ids) = async { this.Update(entity, ids) } |> Async.StartAsTask
    member this.DeleteAsync(entity, [<ParamArray>] ids) = async { this.Delete(entity, ids) } |> Async.StartAsTask
    member this.DeleteAsync(ids) = async { this.Delete(ids) } |> Async.StartAsTask
    member this.SingleEntityAsync([<ParamArray>] ids : obj []) = async { return this.Entity(ids) } |> Async.StartAsTask
    member this.SingleEntityAsync(expr : Expression<Func<'TEntity, bool>>) = async { return this.Entity(expr) } |> Async.StartAsTask
