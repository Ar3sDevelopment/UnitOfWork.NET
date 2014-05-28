using System;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Caelan.Frameworks.BIZ.Interfaces;
using Caelan.Frameworks.DAL.Interfaces;

namespace Caelan.Frameworks.BIZ.Classes
{
    public static class GenericBusinessBuilder
    {
        public static BaseDTOBuilder<TSource, TDestination> GenericDTOBuilder<TSource, TDestination>()
            where TSource : class, IEntity, new()
            where TDestination : class, IDTO, new()
        {
            var builder = new BaseDTOBuilder<TSource, TDestination>();

            var customBuilder = (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetTypes().SingleOrDefault(t => t.BaseType == builder.GetType()) ?? (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetReferencedAssemblies().OrderBy(t => t.Name).Select(Assembly.Load).SelectMany(assembly => assembly.GetTypes().Where(t => t.BaseType == builder.GetType())).SingleOrDefault();

            if (customBuilder != null) return Activator.CreateInstance(customBuilder) as BaseDTOBuilder<TSource, TDestination>;

            if (Mapper.FindTypeMapFor<TSource, TDestination>() == null) Mapper.AddProfile(builder);

            return builder;
        }

        public static BaseEntityBuilder<TSource, TDestination> GenericEntityBuilder<TSource, TDestination>()
            where TSource : class, IDTO, new()
            where TDestination : class, IEntity, new()
        {
            var builder = new BaseEntityBuilder<TSource, TDestination>();

            var customBuilder = (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetTypes().SingleOrDefault(t => t.BaseType == builder.GetType()) ?? (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetReferencedAssemblies().OrderBy(t => t.Name).Select(Assembly.Load).SelectMany(assembly => assembly.GetTypes().Where(t => t.BaseType == builder.GetType())).SingleOrDefault();

            if (customBuilder != null) return Activator.CreateInstance(customBuilder) as BaseEntityBuilder<TSource, TDestination>;

            if (Mapper.FindTypeMapFor<TSource, TDestination>() == null) Mapper.AddProfile(builder);

            return builder;
        }
    }
}
