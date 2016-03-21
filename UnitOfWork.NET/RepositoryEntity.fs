namespace UnitOfWork.NET.Classes

open System
open System.Collections.Generic
open System.Linq
open System.Linq.Expressions
open UnitOfWork.NET.Interfaces

[<AbstractClass>]
type Repository<'T>(manager) = 
    inherit Repository(manager)
    
    interface IRepository<'T> with
        member this.Data = this.Data
        member this.Element(expr : Func<'T, bool>) = this.Element expr
        member this.All() = this.All()
        member this.All whereExpr = whereExpr |> this.All
        member this.Exists expr = expr |> this.Exists
        member this.Count expr = expr |> this.Count
    
    member __.Data = manager.Data<'T>()
    member this.Element expr = expr |> this.Data.FirstOrDefault
    member this.All() = this.Data
    member this.All(whereExpr : Func<'T, bool>) = this.Data.Where(whereExpr)
    member this.Exists expr = this.Data.Any expr
    member this.Count expr = this.Data.Count expr
    member this.ElementAsync(expr : Func<'T, bool>) = async { return this.Element(expr) } |> Async.StartAsTask