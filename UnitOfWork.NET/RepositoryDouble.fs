namespace UnitOfWork.NET.Classes

open Caelan.DynamicLinq.Classes
open Caelan.DynamicLinq.Extensions
open ClassBuilder.Classes
open ClassBuilder.Interfaces
open System
open System.Collections.Generic
open System.Data.Linq
open System.Linq
open System.Linq.Dynamic
open System.Linq.Expressions
open System.Reflection
open UnitOfWork.NET.Interfaces

type Repository<'TSource, 'TDestination when 'TSource : not struct and 'TDestination : not struct>(manager) = 
    inherit Repository<'TSource>(manager : IUnitOfWork)
    
    interface IRepository<'TSource, 'TDestination> with
        member this.ElementBuilt(expr : Func<'TSource, bool>) = expr |> this.ElementBuilt
        member this.AllBuilt() = this.AllBuilt()
        member this.AllBuilt whereExpr = whereExpr |> this.AllBuilt
        member this.DataSource(take, skip, sort, filter, whereFunc) = this.DataSource(take, skip, sort, filter, whereFunc)
    
    member val DestinationMapper : IMapper<'TSource, 'TDestination> option = None with get, set
    member val SourceMapper : IMapper<'TDestination, 'TSource> option = None with get, set
    member this.ElementBuilt(expr : Func<'TSource, bool>) = Builder.Build(this.Element(expr)).To<'TDestination>()
    member this.AllBuilt() = Builder.BuildList(this.All()).ToList<'TDestination>()
    member this.AllBuilt(whereExpr) = Builder.BuildList(this.All(whereExpr)).ToList<'TDestination>()
    member this.DataSource(take, skip, sort, filter, whereFunc) = this.DataSource(take, skip, sort, filter, whereFunc, fun t -> Builder.BuildList(t).ToList<'TDestination>())
    
    member private this.DataSource(take, skip, sort, filter, whereFunc, buildFunc : seq<'TSource> -> seq<'TDestination>) = 
        let orderBy = 
            query { 
                for item in (typeof<'TSource>).GetProperties(BindingFlags.Instance ||| BindingFlags.Public) do
                    select item.Name
                    headOrDefault
            }
        
        let queryResult = 
            let res = 
                match orderBy |> Option.ofObj with
                | None -> this.All(whereFunc)
                | Some(defaultSort) -> this.All(whereFunc).OrderBy(defaultSort)
            res.AsQueryable().ToDataSourceResult(take, skip, sort, filter)
        
        DataSourceResult<'TDestination>(Data = (buildFunc (queryResult.Data)).ToList(), Total = queryResult.Total)
    
    member this.AllBuiltAsync(whereExpr : Func<'TSource, bool>) = async { return this.AllBuilt(whereExpr) } |> Async.StartAsTask
    member this.AllBuiltAsync() = async { return this.AllBuilt() } |> Async.StartAsTask
    member this.DataSourceAsync(take : int, skip : int, sort : ICollection<Sort>, filter : Filter, whereFunc : Func<'TSource, bool>) = async { return this.DataSource(take, skip, sort, filter, whereFunc) } |> Async.StartAsTask
    member this.ElementBuiltAsync(expr : Func<'TSource, bool>) = async { return this.ElementBuilt(expr) } |> Async.StartAsTask
