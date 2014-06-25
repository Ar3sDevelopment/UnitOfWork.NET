namespace Caelan.Frameworks.BIZ.Classes

    open System
    open System.Data.Entity
    open System.Runtime.CompilerServices
    open System.Collections.Generic
    open System.Linq
    open System.Linq.Expressions
    open System.Reflection
    open AutoMapper
    open AutoMapper.Internal
    open Caelan.Frameworks.DAL.Interfaces
    open Caelan.Frameworks.Common.Classes
    open Caelan.Frameworks.Common.Extenders
    open Caelan.Frameworks.BIZ.Interfaces
    open Caelan.DynamicLinq.Classes
    open Caelan.DynamicLinq.Extensions

    [<AbstractClass; Sealed>]
    type GenericBusinessBuilder() =
        static member GenericDTOBuilder<'TEntity, 'TDTO when 'TEntity :> IEntity and 'TDTO :> IDTO and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>() =
            GenericBuilder.CreateGenericBuilder<BaseDTOBuilder<'TEntity, 'TDTO>, 'TEntity, 'TDTO>()

        static member GenericEntityBuilder<'TDTO, 'TEntity when 'TEntity :> IEntity and 'TDTO :> IDTO and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null>() =
            GenericBuilder.CreateGenericBuilder<BaseEntityBuilder<'TDTO, 'TEntity>, 'TDTO, 'TEntity>()

    and BaseDTOBuilder<'TSource, 'TDestination when 'TSource :> IEntity and 'TDestination :> IDTO and 'TSource : equality and 'TSource : null and 'TDestination : equality and 'TDestination : null>() =
        inherit BaseBuilder<'TSource, 'TDestination>()

        abstract member BuildFull : 'TSource -> 'TDestination
        default this.BuildFull(source : 'TSource) =
            let dest = Activator.CreateInstance<'TDestination>()

            this.Build(source, ref dest)

            dest

        abstract member BuildFull : 'TSource * 'TDestination byref -> unit
        default this.BuildFull(source : 'TSource, destination : 'TDestination byref) =
            this.Build(source, ref destination)

        abstract member BuildFullList : seq<'TSource> -> seq<'TDestination>
        default this.BuildFullList(sourceList : seq<'TSource>) =
            sourceList |> Seq.map (fun t -> this.BuildFull(t))

        override this.AfterBuild(source : 'TSource, destination : 'TDestination ref) =
            base.AfterBuild(source, destination)

            let destType = typedefof<'TDestination>
            let sourceType = typedefof<'TSource>
            let properties = destType.GetProperties(BindingFlags.Public ||| BindingFlags.Instance) |> Seq.filter (fun t -> t.PropertyType.IsPrimitive = false && t.PropertyType.IsValueType = false && t.PropertyType.Equals(typedefof<string>) = false && t.PropertyType.IsEnumerableType())

            for prop in properties do
                if Mapper.FindTypeMapFor<'TSource, 'TDestination>().GetPropertyMaps().Any(fun t -> t.IsIgnored() && t.DestinationProperty.Name = prop.Name) = false then
                    let sourceProp = sourceType.GetProperty(prop.Name, BindingFlags.Public ||| BindingFlags.Instance)

                    if sourceProp <> null then
                        if sourceProp.PropertyType.GetInterfaces().Contains(typedefof<IEntity>) && prop.PropertyType.GetInterfaces().Contains(typedefof<IDTO>) then
                            let builderType = typedefof<GenericBusinessBuilder>
                            let builderMethod = builderType.GetMethod("GenericDTOBuilder", BindingFlags.Public ||| BindingFlags.Static).MakeGenericMethod(sourceProp.PropertyType, prop.PropertyType)
                            let builder = builderMethod.Invoke(null, null)
                            let build = builder.GetType().GetMethods(BindingFlags.Public ||| BindingFlags.Instance).Single(fun t -> t.GetParameters().Count() = 1 && t.Name = "Build")

                            prop.SetValue(destination, build.Invoke(builder, [|sourceProp.GetValue(source, null)|]), null)
                        else
                            let builderType = typedefof<GenericBuilder>
                            let builderMethod = builderType.GetMethod("Create", BindingFlags.Public ||| BindingFlags.Static).MakeGenericMethod(sourceProp.PropertyType, prop.PropertyType)
                            let builder = builderMethod.Invoke(null, null)
                            let build = builder.GetType().GetMethods(BindingFlags.Public ||| BindingFlags.Instance).Single(fun t -> t.GetParameters().Count() = 1 && t.Name = "Build")

                            prop.SetValue(destination, build.Invoke(builder, [|sourceProp.GetValue(source, null)|]), null)
                            

        override this.AddMappingConfigurations(mappingExpression : IMappingExpression<'TSource, 'TDestination>) =
            base.AddMappingConfigurations(mappingExpression)

            AutoMapperExtender.IgnoreAllLists(mappingExpression)

    and BaseEntityBuilder<'TSource, 'TDestination when 'TSource :> IDTO and 'TDestination :> IEntity and 'TSource : equality and 'TSource : null and 'TDestination : equality and 'TDestination : null>() =
        inherit BaseBuilder<'TSource, 'TDestination>()
        override this.AddMappingConfigurations(mappingExpression : IMappingExpression<'TSource, 'TDestination>) =
            base.AddMappingConfigurations(mappingExpression)

            AutoMapperExtender.IgnoreAllNonPrimitive(mappingExpression)

    [<AbstractClass>]
    type BaseRepository(manager) =
        interface IBaseRepository
        member this.UnitOfWork : BaseUnitOfWork = manager

        member this.GetUnitOfWork() =
            this.UnitOfWork

        member this.GetUnitOfWork<'T when 'T :> BaseUnitOfWork>() =
            this.UnitOfWork :?> 'T

    and [<AbstractClass>]
        BaseRepository<'TEntity, 'TDTO, 'TKey when 'TKey :> IEquatable<'TKey> and 'TEntity :> IEntity<'TKey>  and 'TEntity : not struct and 'TDTO :> IDTO<'TKey> and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TKey : equality>(manager) =
            inherit BaseRepository(manager)

            abstract member DbSetFunc : unit -> Func<DbContext, DbSet<'TEntity>>
            
            member this.DbSetFuncGetter() =
                this.DbSetFunc()

            abstract member Set : unit -> DbSet<'TEntity>
            default this.Set() =
                this.UnitOfWork.GetDbSet(this)

            abstract member All : unit -> IQueryable<'TEntity>
            default this.All() =
                this.Set() :> IQueryable<'TEntity>

            abstract member All : whereExpr : Expression<Func<'TEntity, bool>> -> IQueryable<'TEntity>
            default this.All(whereExpr : Expression<Func<'TEntity, bool>>) =
                match whereExpr with
                | null -> this.All()
                | _ -> this.Set().Where(whereExpr)

            abstract member All : int * int * seq<Sort> * Filter * Expression<Func<'TEntity, bool>> -> DataSourceResult<'TDTO>
            default this.All(take : int, skip : int, sort : seq<Sort>, filter : Filter, whereFunc : Expression<Func<'TEntity, bool>>) =
                let queryResult = this.All(whereFunc).OrderBy(fun t -> t.ID).ToDataSourceResult(take, skip, sort, filter)
                let result = DataSourceResult<'TDTO>()

                result.Data <- this.DTOBuilder().BuildFullList(queryResult.Data)
                result.Total <- queryResult.Total

                result

            abstract member DTOBuilder : unit -> BaseDTOBuilder<'TEntity, 'TDTO>
            default this.DTOBuilder() =
                GenericBusinessBuilder.GenericDTOBuilder<'TEntity, 'TDTO>()

            abstract member EntityBuilder : unit -> BaseEntityBuilder<'TDTO, 'TEntity>
            default this.EntityBuilder() =
                GenericBusinessBuilder.GenericEntityBuilder<'TDTO, 'TEntity>()

            abstract member Single : 'TKey -> 'TDTO
            default this.Single(id : 'TKey) =
                this.DTOBuilder().BuildFull(
                    match this.Set() |> Seq.tryFind (fun t -> t.ID.Equals(id)) with
                    | None -> null
                    | Some(value) -> value)
    and [<AbstractClass>]
        BaseUnitOfWork() =
            abstract member Context : unit -> DbContext
            default this.Context() = Unchecked.defaultof<DbContext>

            member this.GetDbSet<'TEntity, 'TDTO, 'TKey when 'TKey :> IEquatable<'TKey> and 'TEntity :> IEntity<'TKey>  and 'TEntity : not struct and 'TDTO :> IDTO<'TKey> and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TKey : equality>(repository : BaseRepository<'TEntity, 'TDTO, 'TKey>) =
                repository.DbSetFuncGetter().Invoke(this.Context())

            member this.SaveChanges() =
                this.Context().SaveChanges()
            
    [<AbstractClass>]
    type BaseCRUDRepository<'TEntity, 'TDTO, 'TKey when 'TKey :> IEquatable<'TKey> and 'TEntity :> IEntity<'TKey>  and 'TEntity : not struct and 'TDTO :> IDTO<'TKey> and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TKey : equality>(manager) =
        inherit BaseRepository<'TEntity, 'TDTO, 'TKey>(manager)

        abstract member Insert : 'TDTO -> unit
        default this.Insert(dto : 'TDTO) =
            this.Set().Add(this.EntityBuilder().Build(dto)) |> ignore
            
        abstract member Update : 'TDTO -> unit
        default this.Update(dto : 'TDTO) =
            let entity =
                match this.Set() |> Seq.tryFind (fun t -> t.ID = dto.ID) with
                | None -> ref null
                | Some(value) -> ref value

            this.EntityBuilder().Build(dto, entity)

        abstract member Delete : 'TDTO -> unit
        default this.Delete(dto : 'TDTO) =
            let entity =
                match this.Set() |> Seq.tryFind (fun t -> t.ID = dto.ID) with
                | None -> null
                | Some(value) -> value

            this.Set().Remove(entity) |> ignore

        abstract member Delete : 'TKey -> unit
        default this.Delete(id : 'TKey) =
            this.Delete(this.Single(id))