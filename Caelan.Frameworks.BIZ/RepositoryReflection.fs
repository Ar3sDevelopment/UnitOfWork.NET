namespace Caelan.Frameworks.BIZ.Modules

open Caelan.Frameworks.BIZ.Interfaces
open Caelan.Frameworks.Common.Extenders
open Caelan.Frameworks.Common.Helpers
open System
open System.Reflection

module internal RepositoryReflection = 
    let FindRepositoryInAssemblies<'T when 'T :> IRepository> ([<ParamArray>] args : obj []) (baseType : Type) = 
        let typeEqualsTo t1 t2 = t2 = t1
        let isTypeAssignableTo t1 (t2 : Type) = t1 |> t2.IsAssignableFrom
        let typeSameGeneric (t1 : Type) (t2 : Type) = t1.IsGenericTypeDefinition && t1.GetGenericArguments().Length = t2.GenericTypeArguments.Length
        
        let makeGenericSafe (t : Type) types = 
            try 
                t.MakeGenericType(types)
            with _ -> null
        
        let rec compareTypes comparer (type1 : Type) (type2 : Type) = 
            match type1 with
            | null when type2 |> (isNull >> not) -> false
            | null when type2 |> isNull -> true
            | _ when (type1, type2) ||> comparer -> true
            | _ when (type1, type2) ||> typeSameGeneric -> 
                let genericType = (type1, type2.GenericTypeArguments) ||> makeGenericSafe
                (genericType, type2) ||> compareTypes comparer
            | _ when (type2, type1) ||> typeSameGeneric -> 
                let genericType = (type2, type1.GenericTypeArguments) ||> makeGenericSafe
                (type1, genericType) ||> compareTypes comparer
            | _ -> false
        
        let findRepositoryInAssembly (assembly : Assembly) = 
            assembly |> MemoizeHelper.Memoize(fun a -> 
                            let types = a.GetTypes() |> Seq.filter (fun t -> not t.IsInterface && not t.IsAbstract)
                            match types |> Seq.tryFind (fun t -> (t, baseType) ||> compareTypes typeEqualsTo) with
                            | None -> types |> Seq.tryFind (fun t -> (t, baseType) ||> compareTypes isTypeAssignableTo)
                            | t -> t)
        
        let rec getRepository asmList = 
            match asmList with
            | [] -> null
            | head :: tail -> 
                match head |> findRepositoryInAssembly with
                | Some repo when repo.ContainsGenericParameters -> repo.MakeGenericType(baseType.GenericTypeArguments)
                | Some repo -> repo
                | None -> tail |> getRepository
        
        let repoType = 
            typedefof<'T>.GenericTypeArguments
            |> Seq.map (fun t -> t.Assembly)
            |> Seq.append [| typeof<'T>.Assembly 
                             AssemblyHelper.GetWebEntryAssembly()
                             Assembly.GetEntryAssembly()
                             Assembly.GetCallingAssembly()
                             Assembly.GetExecutingAssembly() |]
            |> Seq.filter (isNull >> not)
            |> Seq.distinct
            |> List.ofSeq
            |> getRepository
        
        Activator.CreateInstance<'T>(repoType, args)
