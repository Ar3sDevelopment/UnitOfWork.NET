namespace Caelan.Frameworks.BIZ.Classes

open System
open System.Linq
open System.Reflection
open AutoMapper
open AutoMapper.Internal
open Caelan.Frameworks.DAL.Interfaces
open Caelan.Frameworks.Common.Classes
open Caelan.Frameworks.Common.Extenders
open Caelan.Frameworks.BIZ.Interfaces

[<Sealed>]
[<AbstractClass>]
type GenericBusinessBuilder() = 
    static member GenericDTOBuilder<'TEntity, 'TDTO when 'TEntity :> IEntity and 'TDTO :> IDTO and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>() = 
        GenericBuilder.CreateGenericBuilder<BaseDTOBuilder<'TEntity, 'TDTO>, 'TEntity, 'TDTO>()
    static member GenericEntityBuilder<'TDTO, 'TEntity when 'TEntity :> IEntity and 'TDTO :> IDTO and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>() = 
        GenericBuilder.CreateGenericBuilder<BaseEntityBuilder<'TDTO, 'TEntity>, 'TDTO, 'TEntity>()

and BaseDTOBuilder<'TSource, 'TDestination when 'TSource :> IEntity and 'TDestination :> IDTO and 'TSource : equality and 'TSource : null and 'TDestination : equality and 'TDestination : null>() = 
    inherit BaseBuilder<'TSource, 'TDestination>()
    abstract BuildFull : 'TSource -> 'TDestination
    
    override this.BuildFull(source) = 
        match source with
        | null -> Unchecked.defaultof<'TDestination>
        | _ -> 
            let dest = ref Unchecked.defaultof<'TDestination>
            if (box dest = null) then dest := Activator.CreateInstance<'TDestination>()
            this.BuildFull(source, dest)
            !dest
    
    abstract BuildFull : 'TSource * 'TDestination byref -> unit
    override this.BuildFull(source, destination) = this.Build(source, &destination)
    abstract BuildFullList : seq<'TSource> -> seq<'TDestination>
    override this.BuildFullList(sourceList) = sourceList |> Seq.map (fun t -> this.BuildFull(t))
    member this.BuildFullAsync(source) = async { return this.BuildFull(source) } |> Async.StartAsTask
    
    member this.BuildFullAsync(source, destination : 'TDestination byref) = 
        let dest = ref destination
        async { return this.BuildFull(source, dest) } |> Async.StartAsTask
    
    member this.BuildFullListAsync(source) = async { return this.BuildFullList(source) } |> Async.StartAsTask
    
    override this.AfterBuild(source, destination) = 
        //base.AfterBuild(source, &destination)
        let destType = typedefof<'TDestination>
        let sourceType = typedefof<'TSource>
        let properties = 
            destType.GetProperties(BindingFlags.Public ||| BindingFlags.Instance) 
            |> Seq.filter 
                   (fun t -> 
                   (t.PropertyType.IsPrimitive || t.PropertyType.IsValueType || t.PropertyType.Equals(typedefof<string>)) = false 
                   && t.PropertyType.IsEnumerableType() = false)
        for prop in properties do
            if Mapper.FindTypeMapFor<'TSource, 'TDestination>().GetPropertyMaps()
                   .Any(fun t -> t.IsIgnored() && t.DestinationProperty.Name = prop.Name) = false then 
                let sourceProp = sourceType.GetProperty(prop.Name, BindingFlags.Public ||| BindingFlags.Instance)
                if sourceProp <> null then 
                    if sourceProp.PropertyType.GetInterfaces().Contains(typedefof<IEntity>) 
                       && prop.PropertyType.GetInterfaces().Contains(typedefof<IDTO>) then 
                        let builderGenerator = 
                            (typedefof<GenericBusinessBuilder>)
                                .GetMethod("GenericDTOBuilder", BindingFlags.Public ||| BindingFlags.Static)
                                .MakeGenericMethod(sourceProp.PropertyType, prop.PropertyType)
                        let builder = builderGenerator.Invoke(null, null)
                        let buildMethod = 
                            builder.GetType().GetMethods(BindingFlags.Public ||| BindingFlags.Instance)
                                   .Single(fun t -> t.GetParameters().Count() = 1 && t.Name = "Build")
                        let sourceValue = sourceProp.GetValue(source, null)
                        if (sourceValue <> null) then 
                            let destValue = buildMethod.Invoke(builder, [| sourceValue |])
                            prop.SetValue(destination, destValue)
                    else 
                        let builderGenerator = 
                            (typedefof<GenericBuilder>).GetMethod("Create", BindingFlags.Public ||| BindingFlags.Static)
                                .MakeGenericMethod(sourceProp.PropertyType, prop.PropertyType)
                        let builder = builderGenerator.Invoke(null, null)
                        let buildMethod = 
                            builder.GetType().GetMethods(BindingFlags.Public ||| BindingFlags.Instance)
                                   .Single(fun t -> t.GetParameters().Count() = 1 && t.Name = "Build")
                        let sourceValue = sourceProp.GetValue(source, null)
                        if (sourceValue <> null) then 
                            let destValue = buildMethod.Invoke(builder, [| sourceValue |])
                            prop.SetValue(destination, destValue)
    
    override this.AddMappingConfigurations(mappingExpression) = 
        base.AddMappingConfigurations(mappingExpression)
        AutoMapperExtender.IgnoreAllLists(mappingExpression)

and BaseEntityBuilder<'TSource, 'TDestination when 'TSource :> IDTO and 'TDestination :> IEntity and 'TSource : equality and 'TSource : null and 'TDestination : equality and 'TDestination : null>() = 
    inherit BaseBuilder<'TSource, 'TDestination>()
    override this.AddMappingConfigurations(mappingExpression) = 
        base.AddMappingConfigurations(mappingExpression)
        AutoMapperExtender.IgnoreAllNonPrimitive(mappingExpression)
