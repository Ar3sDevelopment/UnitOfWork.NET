namespace Caelan.Frameworks.BIZ.Classes

open System
open System.Data.Entity
open System.Linq
open System.Reflection
open System.Collections.Generic
open Caelan.Frameworks.BIZ.Interfaces

[<AllowNullLiteral>]
type UnitOfWork(context : DbContext) = 
    
    let memoize f = 
        let dict = new System.Collections.Generic.Dictionary<_, _>()
        fun n -> 
            match dict.TryGetValue(n) with
            | (true, v) -> v
            | _ -> 
                let temp = f (n)
                dict.Add(n, temp)
                temp
    
    let findRepositoryInAssemblies ([<ParamArray>] args : obj []) (baseType : Type) = 
        let typeEqualsTo = (fun t1 (t2 : Type) -> t2 = t1)
        let isTypeAssignableTo = (fun t1 (t2 : Type) -> t2.IsAssignableFrom(t1))
        let typeSameGeneric = 
            (fun (t1 : Type) (t2 : Type) -> 
            t1.IsGenericTypeDefinition && t1.GetGenericArguments().Length = t2.GenericTypeArguments.Length)
        
        let makeGenericSafe (t : Type) (types : Type []) = 
            try 
                t.MakeGenericType(types)
            with _ -> null
        
        let rec compareTypes comparer (type1 : Type) (type2 : Type) = 
            match type1 with
            | null when type2 <> null -> false
            | null when type2 = null -> true
            | _ when (type1, type2) ||> comparer -> true
            | _ when (type1, type2) ||> typeSameGeneric -> 
                ((type1, type2.GenericTypeArguments) ||> makeGenericSafe, type2) ||> compareTypes comparer
            | _ when (type2, type1) ||> typeSameGeneric -> 
                (type1, (type2, type1.GenericTypeArguments) ||> makeGenericSafe) ||> compareTypes comparer
            | _ -> false
        
        let findRepositoryInAssembly (assembly : Assembly) = 
            let types = assembly.GetTypes() |> Seq.filter (fun t -> not t.IsInterface && not t.IsAbstract)
            match types |> Seq.tryFind (fun t -> (t, baseType) ||> compareTypes typeEqualsTo) with
            | None -> types |> Seq.tryFind (fun t -> (t, baseType) ||> compareTypes isTypeAssignableTo)
            | t -> t
        
        let rec getRepository asmList = 
            match asmList with
            | head :: tail -> 
                match head |> findRepositoryInAssembly with
                | None -> tail |> getRepository
                | Some(repo) when repo.ContainsGenericParameters -> repo.MakeGenericType(baseType.GenericTypeArguments)
                | Some(repo) -> repo
            | [] -> null
        
        let repoType = 
            [ Assembly.GetExecutingAssembly()
              Assembly.GetEntryAssembly()
              Assembly.GetCallingAssembly() ]
            |> List.filter (fun t -> t <> null)
            |> getRepository
        
        Activator.CreateInstance(repoType, args) :?> IRepository
    
    interface IUnitOfWork with
        member this.SaveChanges() = this.SaveChanges()
        member this.DbSet<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>() = 
            this.DbSet<'TEntity>()
        member this.Entry<'TEntity>(entity) = this.Entry<'TEntity>(entity)
        member this.CustomRepository<'TRepository when 'TRepository :> IRepository>() = 
            this.CustomRepository<'TRepository>()
        member this.Repository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>() = 
            this.Repository<'TEntity>()
        member this.Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>() = 
            this.Repository<'TEntity, 'TDTO>()
        member this.Repository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct>() = 
            this.Repository<'TEntity, 'TDTO, 'TListDTO>()
        member this.Transaction(body : Action<IUnitOfWork>) = this.Transaction(body)
        member this.TransactionSaveChanges(body : Action<IUnitOfWork>) = this.TransactionSaveChanges(body)
    
    member __.SaveChanges() = context.SaveChanges()
    member __.SaveChangesAsync() = async { return! context.SaveChangesAsync() |> Async.AwaitTask } |> Async.StartAsTask
    
    member this.CustomRepository<'TRepository when 'TRepository :> IRepository>() = 
        typeof<'TRepository> |> memoize (fun tp -> 
                                    (match this.GetType().GetProperties(BindingFlags.Instance) 
                                           |> Seq.tryFind (fun t -> t.PropertyType = tp) with
                                     | Some(repositoryProp) -> repositoryProp.GetValue(this)
                                     | None -> Activator.CreateInstance(tp, this)) :?> 'TRepository)
    
    member this.Repository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>() = 
        typeof<IRepository<'TEntity>> 
        |> memoize (fun t -> t |> findRepositoryInAssemblies [| this |] :?> IRepository<'TEntity>)
    member this.Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>() = 
        typeof<IRepository<'TEntity, 'TDTO>> 
        |> memoize (fun t -> t |> findRepositoryInAssemblies [| this |] :?> IRepository<'TEntity, 'TDTO>)
    member this.Repository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct>() = 
        typeof<IListRepository<'TEntity, 'TDTO, 'TListDTO>> 
        |> memoize (fun t -> t |> findRepositoryInAssemblies [| this |] :?> IListRepository<'TEntity, 'TDTO, 'TListDTO>)
    member __.Entry<'TEntity>(entity : 'TEntity) = context.Entry(entity)
    member __.DbSet<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>() = 
        context.Set<'TEntity>()
    
    member this.Transaction(body : Action<IUnitOfWork>) = 
        using (context.Database.BeginTransaction()) (fun transaction -> 
            try 
                body.Invoke(this)
                transaction.Commit()
            with _ -> transaction.Rollback())
    
    member this.TransactionSaveChanges(body : Action<IUnitOfWork>) = 
        using (context.Database.BeginTransaction()) (fun transaction -> 
            try 
                body.Invoke(this)
                let res = context.SaveChanges() <> 0
                transaction.Commit()
                res
            with _ -> 
                transaction.Rollback()
                false)
    
    interface IDisposable with
        member this.Dispose() = this.Dispose()
    
    abstract Dispose : unit -> unit
    override __.Dispose() = ()

type UnitOfWork<'TContext when 'TContext :> DbContext> private (context) = 
    inherit UnitOfWork(context)
    
    override this.Dispose() = 
        context.Dispose()
        base.Dispose()
    
    new() = 
        let context = Activator.CreateInstance<'TContext>()
        new UnitOfWork<'TContext>(context)
