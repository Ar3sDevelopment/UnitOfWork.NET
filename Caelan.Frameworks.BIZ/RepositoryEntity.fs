namespace Caelan.Frameworks.BIZ.Classes

open System
open System.Linq
open System.Linq.Expressions
open Caelan.Frameworks.BIZ.Interfaces

type Repository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>(manager : IUnitOfWork) = 
    inherit Repository(manager : IUnitOfWork)
    
    interface IRepository<'TEntity> with
        member this.Set = this.Set
        member this.SingleEntity([<ParamArray>] ids : obj []) = this.SingleEntity ids
        member this.SingleEntity(expr : Expression<Func<'TEntity, bool>>) = this.SingleEntity expr
        member this.All() = this.All()
        member this.All whereExpr = this.All whereExpr
        member this.Insert(entity : 'TEntity) = this.Insert entity
        member this.Update(entity : 'TEntity, [<ParamArray>] ids) = this.Update(entity, ids)
        member this.Delete([<ParamArray>] ids : obj []) = this.Delete ids
        member this.Exists(expr : Expression<Func<'TEntity, bool>>) = this.Exists expr
    
    member __.Set = manager.DbSet<'TEntity>()
    member this.SingleEntity([<ParamArray>] ids) = this.Set.Find ids
    member this.SingleEntity expr = this.Set.FirstOrDefault expr
    member this.All() = this.Set.AsQueryable()
    member this.All(whereExpr : Expression<Func<'TEntity, bool>>) = this.Set.Where(whereExpr).AsQueryable()
    member this.Exists expr = this.Set.Any(expr)
    
    abstract Insert : entity:'TEntity -> 'TEntity
    abstract Update : 'TEntity * [<ParamArray>]ids:obj [] -> unit
    abstract Delete : [<ParamArray>]ids:obj [] -> unit
    override this.Insert entity = this.Set.Add(entity)
    override this.Update(entity, [<ParamArray>] ids) = manager.Entry(this.Set.Find ids).CurrentValues.SetValues(entity)
    override this.Delete([<ParamArray>] ids) = ids |> this.SingleEntity |> this.Set.Remove |> ignore
    member this.InsertAsync entity = async { return this.Insert(entity) } |> Async.StartAsTask
    member this.UpdateAsync(entity, ids) = async { this.Update(entity, ids) } |> Async.StartAsTask
    member this.DeleteAsync(entity, [<ParamArray>] ids) = async { this.Delete(entity, ids) } |> Async.StartAsTask
    member this.DeleteAsync(ids) = async { this.Delete(ids) } |> Async.StartAsTask
    member this.SingleEntityAsync([<ParamArray>] ids : obj []) = 
        async { return this.SingleEntity(ids) } |> Async.StartAsTask
    member this.SingleEntityAsync(expr : Expression<Func<'TEntity, bool>>) = 
        async { return this.SingleEntity(expr) } |> Async.StartAsTask
