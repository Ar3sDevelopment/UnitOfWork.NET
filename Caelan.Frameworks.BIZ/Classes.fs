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
        let dest = Activator.CreateInstance<'TDestination>()
        this.Build(source, ref dest)
        dest
    
    abstract BuildFull : 'TSource * 'TDestination byref -> unit
    override this.BuildFull(source, destination) = this.Build(source, ref destination)
    abstract BuildFullList : seq<'TSource> -> seq<'TDestination>
    override this.BuildFullList(sourceList) = sourceList |> Seq.map (fun t -> this.BuildFull(t))
    
    override this.AfterBuild(source, destination) = 
        base.AfterBuild(source, destination)
        let destType = typedefof<'TDestination>
        let sourceType = typedefof<'TSource>
        let properties = 
            destType.GetProperties(BindingFlags.Public ||| BindingFlags.Instance) 
            |> Seq.filter 
                   (fun t -> 
                   t.PropertyType.IsPrimitive = false && t.PropertyType.IsValueType = false 
                   && t.PropertyType.Equals(typedefof<string>) = false && t.PropertyType.IsEnumerableType())
        for prop in properties do
            if Mapper.FindTypeMapFor<'TSource, 'TDestination>().GetPropertyMaps()
                   .Any(fun t -> t.IsIgnored() && t.DestinationProperty.Name = prop.Name) = false then 
                let sourceProp = sourceType.GetProperty(prop.Name, BindingFlags.Public ||| BindingFlags.Instance)
                if sourceProp <> null then 
                    if sourceProp.PropertyType.GetInterfaces().Contains(typedefof<IEntity>) 
                       && prop.PropertyType.GetInterfaces().Contains(typedefof<IDTO>) then 
                        let builderType = typedefof<GenericBusinessBuilder>
                        let builderMethod = 
                            builderType.GetMethod("GenericDTOBuilder", BindingFlags.Public ||| BindingFlags.Static)
                                       .MakeGenericMethod(sourceProp.PropertyType, prop.PropertyType)
                        let builder = builderMethod.Invoke(null, null)
                        let build = 
                            builder.GetType().GetMethods(BindingFlags.Public ||| BindingFlags.Instance)
                                   .Single(fun t -> t.GetParameters().Count() = 1 && t.Name = "Build")
                        prop.SetValue(destination, build.Invoke(builder, [| sourceProp.GetValue(source, null) |]), null)
                    else 
                        let builderType = typedefof<GenericBuilder>
                        let builderMethod = 
                            builderType.GetMethod("Create", BindingFlags.Public ||| BindingFlags.Static)
                                       .MakeGenericMethod(sourceProp.PropertyType, prop.PropertyType)
                        let builder = builderMethod.Invoke(null, null)
                        let build = 
                            builder.GetType().GetMethods(BindingFlags.Public ||| BindingFlags.Instance)
                                   .Single(fun t -> t.GetParameters().Count() = 1 && t.Name = "Build")
                        prop.SetValue(destination, build.Invoke(builder, [| sourceProp.GetValue(source, null) |]), null)
    
    override this.AddMappingConfigurations(mappingExpression) = 
        base.AddMappingConfigurations(mappingExpression)
        AutoMapperExtender.IgnoreAllLists(mappingExpression)

and BaseEntityBuilder<'TSource, 'TDestination when 'TSource :> IDTO and 'TDestination :> IEntity and 'TSource : equality and 'TSource : null and 'TDestination : equality and 'TDestination : null>() = 
    inherit BaseBuilder<'TSource, 'TDestination>()
    override this.AddMappingConfigurations(mappingExpression) = 
        base.AddMappingConfigurations(mappingExpression)
        AutoMapperExtender.IgnoreAllNonPrimitive(mappingExpression)

[<AbstractClass>]
type BaseRepository(manager) = 
    interface IBaseRepository
    member private this.UnitOfWork : BaseUnitOfWorkManager = manager
    member this.GetUnitOfWork() = this.UnitOfWork
    member this.GetUnitOfWork<'T when 'T :> BaseUnitOfWorkManager>() = this.UnitOfWork :?> 'T

and [<AbstractClass>] BaseRepository<'TEntity, 'TDTO, 'TKey when 'TKey :> IEquatable<'TKey> and 'TEntity :> IEntity<'TKey> and 'TEntity : not struct and 'TDTO :> IDTO<'TKey> and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TKey : equality>(manager) = 
    inherit BaseRepository(manager)
    let mutable dbSetFunc : Func<DbContext, DbSet<'TEntity>> = null
    member this.DbSetFunc 
        with set (value) = dbSetFunc <- value
    member this.DbSetFuncGetter() = dbSetFunc
    abstract Set : unit -> DbSet<'TEntity>
    override this.Set() = this.GetUnitOfWork().GetDbSet(this)
    abstract All : unit -> IQueryable<'TEntity>
    override this.All() = this.Set() :> IQueryable<'TEntity>
    abstract All : whereExpr:Expression<Func<'TEntity, bool>> -> IQueryable<'TEntity>
    
    override this.All(whereExpr) = 
        match whereExpr with
        | null -> this.All()
        | _ -> this.Set().Where(whereExpr)
    
    abstract All : int * int * seq<Sort> * Filter * Expression<Func<'TEntity, bool>> -> DataSourceResult<'TDTO>
    
    override this.All(take, skip, sort, filter, whereFunc) = 
        let queryResult = this.All(whereFunc).OrderBy(fun t -> t.ID).ToDataSourceResult(take, skip, sort, filter)
        let result = DataSourceResult<'TDTO>()
        result.Data <- this.DTOBuilder().BuildFullList(queryResult.Data)
        result.Total <- queryResult.Total
        result
    
    abstract DTOBuilder : unit -> BaseDTOBuilder<'TEntity, 'TDTO>
    override this.DTOBuilder() = GenericBusinessBuilder.GenericDTOBuilder<'TEntity, 'TDTO>()
    abstract EntityBuilder : unit -> BaseEntityBuilder<'TDTO, 'TEntity>
    override this.EntityBuilder() = GenericBusinessBuilder.GenericEntityBuilder<'TDTO, 'TEntity>()
    abstract Single : 'TKey -> 'TDTO
    override this.Single(id) = 
        this.DTOBuilder().BuildFull(match this.Set() |> Seq.tryFind (fun t -> t.ID.Equals(id)) with
                                    | None -> null
                                    | Some(value) -> value)

and [<AbstractClass>] BaseUnitOfWorkManager(uow : IUnitOfWork) = 
    let unitOfWork = uow
    member this.GetDbSet<'TEntity, 'TDTO, 'TKey when 'TKey :> IEquatable<'TKey> and 'TEntity :> IEntity<'TKey> and 'TEntity : not struct and 'TDTO :> IDTO<'TKey> and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TKey : equality>(repository : BaseRepository<'TEntity, 'TDTO, 'TKey>) = 
        repository.DbSetFuncGetter().Invoke(uow.Context())
    member this.SaveChanges() = uow.Context().SaveChanges()
    member this.Entry<'TEntity>(entity : 'TEntity) = uow.Context().Entry(entity)

[<AbstractClass>]
type BaseCRUDRepository<'TEntity, 'TDTO, 'TKey when 'TKey :> IEquatable<'TKey> and 'TEntity :> IEntity<'TKey> and 'TEntity : not struct and 'TDTO :> IDTO<'TKey> and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TKey : equality>(manager, dbSetFunc) = 
    inherit BaseRepository<'TEntity, 'TDTO, 'TKey>(manager)
    abstract Insert : 'TDTO -> unit
    override this.Insert(dto) = this.Set().Add(this.EntityBuilder().Build(dto)) |> ignore
    abstract Update : 'TDTO -> unit
    
    override this.Update(dto) = 
        let entity = 
            match this.Set() |> Seq.tryFind (fun t -> t.ID = dto.ID) with
            | None -> null
            | Some(value) -> value
        
        let newEntity : ref<'TEntity> = ref null
        this.EntityBuilder().Build(dto, newEntity)
        manager.Entry(entity).CurrentValues.SetValues(!newEntity)
    
    abstract Delete : 'TDTO -> unit
    
    override this.Delete(dto : 'TDTO) = 
        let entity = 
            match this.Set() |> Seq.tryFind (fun t -> t.ID = dto.ID) with
            | None -> null
            | Some(value) -> value
        this.Set().Remove(entity) |> ignore
    
    abstract Delete : 'TKey -> unit
    override this.Delete(id) = this.Delete(this.Single(id))
