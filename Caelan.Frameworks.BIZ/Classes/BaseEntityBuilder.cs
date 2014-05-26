using AutoMapper;
using Caelan.Frameworks.BIZ.Interfaces;
using Caelan.Frameworks.Common.Classes;
using Caelan.Frameworks.Common.Extenders;
using Caelan.Frameworks.DAL.Interfaces;

namespace Caelan.Frameworks.BIZ.Classes
{
	public class BaseEntityBuilder<TSource, TDestination> : BaseBuilder<TSource, TDestination>
		where TSource : class, IDTO, new()
		where TDestination : class, IEntity, new()
	{
		protected override void AddMappingConfigurations(IMappingExpression<TSource, TDestination> mappingExpression)
		{
			base.AddMappingConfigurations(mappingExpression);

			mappingExpression.IgnoreAllNonPrimitive();
		}
	}
}
