using System;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Caelan.Frameworks.BIZ.Interfaces;
using Caelan.Frameworks.Common.Classes;
using Caelan.Frameworks.DAL.Interfaces;

namespace Caelan.Frameworks.BIZ.Classes
{
    public static class GenericBusinessBuilder
    {
        public static BaseDTOBuilder<TSource, TDestination> GenericDTOBuilder<TSource, TDestination>()
            where TSource : class, IEntity, new()
            where TDestination : class, IDTO, new()
        {
            return GenericBuilder.CreateGenericBuilder<BaseDTOBuilder<TSource, TDestination>, TSource, TDestination>();
        }

        public static BaseEntityBuilder<TSource, TDestination> GenericEntityBuilder<TSource, TDestination>()
            where TSource : class, IDTO, new()
            where TDestination : class, IEntity, new()
        {
            return GenericBuilder.CreateGenericBuilder<BaseEntityBuilder<TSource, TDestination>, TSource, TDestination>();
        }
    }
}
