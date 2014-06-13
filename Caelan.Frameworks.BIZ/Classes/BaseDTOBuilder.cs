using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Internal;
using Caelan.Frameworks.BIZ.Interfaces;
using Caelan.Frameworks.Common.Classes;
using Caelan.Frameworks.Common.Extenders;
using Caelan.Frameworks.DAL.Interfaces;

namespace Caelan.Frameworks.BIZ.Classes
{
    public class BaseDTOBuilder<TSource, TDestination> : BaseBuilder<TSource, TDestination>
        where TDestination : class, IDTO, new()
        where TSource : class, IEntity, new()
    {
        protected override void AddMappingConfigurations(IMappingExpression<TSource, TDestination> mappingExpression)
        {
            base.AddMappingConfigurations(mappingExpression);

            mappingExpression.IgnoreAllLists();
        }

        public virtual TDestination BuildFull(TSource source)
        {
            var dest = new TDestination();

            BuildFull(source, ref dest);

            return dest;
        }

        public virtual void BuildFull(TSource source, ref TDestination destination)
        {
            Build(source, ref destination);
        }

        public IEnumerable<TDestination> BuildFullList(IEnumerable<TSource> sourceList)
        {
            return sourceList == null ? null : sourceList.Select(BuildFull);
        }

        public async Task<TDestination> BuildFullAsync(TSource source)
        {
            return await Task.Run(() => BuildFull(source));
        }

        public async Task<IEnumerable<TDestination>> BuildFullListAsync(IEnumerable<TSource> sourceList)
        {
            return await Task.Run(() => BuildFullList(sourceList));
        }

        public override void AfterBuild(TSource source, ref TDestination destination)
        {
            base.AfterBuild(source, ref destination);

            foreach (var prop in from prop in typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance) let propType = prop.PropertyType where !(propType.IsPrimitive || propType.IsValueType || propType == typeof(string)) && !propType.IsEnumerableType() select prop)
            {
                if (Mapper.FindTypeMapFor<TSource, TDestination>().GetPropertyMaps().Any(t => t.IsIgnored() && t.DestinationProperty.Name == prop.Name)) continue;

                var sourceProp = typeof(TSource).GetProperty(prop.Name, BindingFlags.Instance | BindingFlags.Public);

                if (sourceProp == null) continue;

                if (sourceProp.PropertyType.GetInterfaces().Contains(typeof(IEntity)) && prop.PropertyType.GetInterfaces().Contains(typeof(IDTO)))
                {
                    var method = typeof(GenericBusinessBuilder).GetMethod("GenericDTOBuilder", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(sourceProp.PropertyType, prop.PropertyType);
                    var builder = method.Invoke(null, null);
                    var buildMethod = builder.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).Single(t => t.GetParameters().Count() == 1 && t.Name == "Build");

                    prop.SetValue(destination, buildMethod.Invoke(builder, new[] { sourceProp.GetValue(source, null) }), null);
                }
                else
                {
                    var method = typeof(GenericBuilder).GetMethod("Create", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(sourceProp.PropertyType, prop.PropertyType);
                    var builder = method.Invoke(null, null);
                    var buildMethod = builder.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).Single(t => t.GetParameters().Count() == 1 && t.Name == "Build");

                    prop.SetValue(destination, buildMethod.Invoke(builder, new[] { sourceProp.GetValue(source, null) }), null);
                }
            }
        }
    }
}
