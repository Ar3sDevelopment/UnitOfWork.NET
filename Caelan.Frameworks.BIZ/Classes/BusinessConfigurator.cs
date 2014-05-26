using System;
using System.Linq;
using System.Reflection;
using AutoMapper;

namespace Caelan.Frameworks.BIZ.Classes
{
	public static class BusinessConfigurator
	{
		public static void AutoMapperConfiguration()
		{
			var profileType = typeof(Profile);
			var profiles = Assembly.GetExecutingAssembly().GetTypes().Where(t => profileType.IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null && !t.IsGenericType).Select(Activator.CreateInstance).Cast<Profile>().ToList();

			Mapper.Initialize(a => profiles.ForEach(a.AddProfile));
		}
	}
}
