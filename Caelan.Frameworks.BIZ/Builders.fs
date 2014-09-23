namespace Caelan.Frameworks.BIZ.Classes

open System
open System.Linq
open System.Reflection
open AutoMapper
open AutoMapper.Internal
open Caelan.Frameworks.Common.Classes
open Caelan.Frameworks.Common.Extenders

[<Sealed>]
[<AbstractClass>]
type GenericBusinessBuilder() = 
    static member GenericDTOBuilder<'TEntity, 'TDTO when 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>() = 
        GenericBuilder.CreateGenericBuilder<BaseDTOBuilder<'TEntity, 'TDTO>, 'TEntity, 'TDTO>()
    static member GenericEntityBuilder<'TDTO, 'TEntity when 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>() = 
        GenericBuilder.CreateGenericBuilder<BaseEntityBuilder<'TDTO, 'TEntity>, 'TDTO, 'TEntity>()

and BaseDTOBuilder<'TSource, 'TDestination when 'TSource : equality and 'TSource : null and 'TDestination : equality and 'TDestination : null>() = 
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
    
    abstract BuildFull : 'TSource * 'TDestination ref -> unit
    override this.BuildFull(source, destination) = this.Build(source, destination)
    abstract BuildFullList : seq<'TSource> -> seq<'TDestination>
    override this.BuildFullList(sourceList) = sourceList |> Seq.map (fun t -> this.BuildFull(t))
    member this.BuildFullAsync(source) = async { return this.BuildFull(source) } |> Async.StartAsTask
    member this.BuildFullAsync(source, destination) = 
        async { return this.BuildFull(source, ref destination) } |> Async.StartAsTask
    member this.BuildFullListAsync(source) = async { return this.BuildFullList(source) } |> Async.StartAsTask
    
    override __.AfterBuild(source, destination) = 
        base.AfterBuild(source, destination)
        let destType = typeof<'TDestination>
        let sourceType = typeof<'TSource>
        for prop in destType.GetProperties(BindingFlags.Public ||| BindingFlags.Instance) 
                    |> Seq.filter 
                           (fun t -> 
                           (t.PropertyType.IsPrimitive || t.PropertyType.IsValueType 
                            || t.PropertyType.Equals(typeof<string>)) = false 
                           && t.PropertyType.IsEnumerableType() = false 
                           && Mapper.FindTypeMapFor<'TSource, 'TDestination>().GetPropertyMaps()
                              |> Seq.exists (fun x -> x.IsIgnored() && x.DestinationProperty.Name = t.Name) = false) do
            let sourceProp = sourceType.GetProperty(prop.Name, BindingFlags.Public ||| BindingFlags.Instance)
            if sourceProp <> null then 
                let builderGenerator = 
                    (typeof<GenericBusinessBuilder>)
                        .GetMethod("GenericDTOBuilder", BindingFlags.Public ||| BindingFlags.Static)
                        .MakeGenericMethod(sourceProp.PropertyType, prop.PropertyType)
                let builder = builderGenerator.Invoke(null, null)
                let buildMethod = 
                    builder.GetType().GetMethods(BindingFlags.Public ||| BindingFlags.Instance)
                           .Single(fun t -> t.GetParameters().Count() = 1 && t.Name = "Build")
                let sourceValue = sourceProp.GetValue(source, null)
                if (sourceValue <> null) then 
                    let destValue = buildMethod.Invoke(builder, [| sourceValue |])
                    prop.SetValue(!destination, destValue)
    
    override __.AddMappingConfigurations(mappingExpression) = 
        base.AddMappingConfigurations(mappingExpression)
        AutoMapperExtender.IgnoreAllLists(mappingExpression)

and BaseEntityBuilder<'TSource, 'TDestination when 'TSource : equality and 'TSource : null and 'TDestination : equality and 'TDestination : null>() = 
    inherit BaseBuilder<'TSource, 'TDestination>()
    override __.AddMappingConfigurations(mappingExpression) = 
        base.AddMappingConfigurations(mappingExpression)
        AutoMapperExtender.IgnoreAllNonPrimitive(mappingExpression)