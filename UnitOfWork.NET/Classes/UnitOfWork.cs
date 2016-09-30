using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Autofac;
using UnitOfWork.NET.Interfaces;
using System.Linq;
using UnitOfWork.NET.Extenders;

namespace UnitOfWork.NET.Classes
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ObservableCollection<Assembly> _assemblies;
		private readonly IContainer _container;

		public UnitOfWork()
		{
			_assemblies = new ObservableCollection<Assembly>();

			var cb = new ContainerBuilder();

			cb.Register(t => this).AsImplementedInterfaces().AsSelf().As<UnitOfWork>();
			cb.RegisterType<Repository>().AsSelf().As<IRepository>().PreserveExistingDefaults();
			cb.RegisterGeneric(typeof(Repository<>)).AsSelf().As(typeof(IRepository<>));
			cb.RegisterGeneric(typeof(Repository<,>)).AsSelf().As(typeof(IRepository<,>));
			cb.RegisterGeneric(typeof(Repository<,,>)).AsSelf().As(typeof(IListRepository<,,>));

			_container = cb.Build();

			RegisterProperties();

			_assemblies.CollectionChanged += (sender, args) =>
			{
				RegisterAssembly(args.NewItems.Cast<Assembly>().ToArray());

				UpdateProperties();
			};

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) _assemblies.Add(assembly);
		}

		protected void RegisterProperties()
		{
			var cb = new ContainerBuilder();

			var fields = GetType().GetFields().ToArray();
			var properties = GetType().GetProperties().ToArray();

			foreach (var type in fields.Select(t => t.FieldType).Union(properties.Select(t => t.PropertyType)).Where(IsRepository)) RegisterRepository(cb, type);

			cb.Update(_container);
		}

		protected void UpdateProperties()
		{
			var fields = GetType().GetFields().ToArray();
			var properties = GetType().GetProperties().ToArray();

			foreach (var field in fields.Where(t => t.FieldType.IsAssignableTo<IRepository>())) field.SetValue(this, _container.ResolveOptional(field.FieldType));
			foreach (var property in properties.Where(t => t.PropertyType.IsAssignableTo<IRepository>())) property.SetValue(this, _container.ResolveOptional(property.PropertyType));
		}

		private bool IsRepository(Type t)
		{
			try
			{
				return t.IsAssignableTo<IRepository>() && !t.IsInterface && !t.IsAbstract && t != typeof(Repository) && ((t.IsGenericType && t.GetGenericTypeDefinition() != typeof(Repository<>) && t.GetGenericTypeDefinition() != typeof(Repository<,>) && t.GetGenericTypeDefinition() != typeof(Repository<,,>)) || !t.IsGenericType) && !_container.IsRegistered(t);
			}
			catch
			{
				return false;
			}
		}

		private void RegisterAssembly(Assembly[] assemblyArr)
		{
			var cb = new ContainerBuilder();

			cb.RegisterAssemblyTypes(assemblyArr.Where(t =>
			{
				try
				{
					return t.GetTypes().Any(IsRepository);
				}
				catch
				{
					return false;
				}
			}).ToArray()).Where(IsRepository).AsSelf().AsImplementedInterfaces();
			cb.Update(_container);

			foreach (var assembly in assemblyArr.SelectMany(t => t.GetReferencedAssemblies()).Select(t =>
			{
				try
				{
					return Assembly.Load(t);
				}
				catch
				{
					return null;
				}
			}).Where(
				t =>
				{
					try
					{
						return t != null && !_assemblies.Contains(t) && t.GetTypes().Any(IsRepository);
					}
					catch
					{
						return false;
					}
				}))
				_assemblies.Add(assembly);
		}

		public void RegisterRepository<TRepository>() where TRepository : IRepository => RegisterRepository(typeof(TRepository));

		public void RegisterRepositories(Type[] repositoryTypes)
		{
			var cb = new ContainerBuilder();

			foreach (var repositoryType in repositoryTypes)
			{
				if (repositoryType.IsInterface || repositoryType.IsAbstract || IsRepositoryRegistered(repositoryType)) return;
				RegisterRepository(cb, repositoryType);
			}

			UpdateContainer(cb);
		}

		protected virtual void RegisterRepository(ContainerBuilder cb, Type repositoryType)
		{
			if (IsRepositoryRegistered(repositoryType)) return;

			if (repositoryType.IsGenericTypeDefinition)
			{
				cb.RegisterGeneric(repositoryType).AsSelf().AsRepository().AsImplementedInterfaces();
			}
			else
			{
				var baseType = repositoryType.BaseType;
				while (baseType.IsAssignableTo<IRepository>() && baseType.GenericTypeArguments.Length <= 0)
				{
					baseType = baseType.BaseType;
				}
				var args = baseType.GenericTypeArguments;
				Type sourceType = null;
				Type destType = null;
				if (args.Length > 0)
				{
					sourceType = args.First();
					if (args.Length > 1)
					{
						destType = args.Skip(1).First();
					}
				}
				cb.RegisterType(repositoryType).AsSelf().AsRepository(sourceType, destType).AsImplementedInterfaces();
			}
		}

		public void RegisterRepository(Type repositoryType)
		{
			RegisterRepositories(new[] { repositoryType });
		}

		private TRepository GetRepository<TRepository>() where TRepository : IRepository
		{
			RegisterRepository<TRepository>();

			return _container.Resolve<TRepository>();
		}

		protected void UpdateContainer(ContainerBuilder cb) => cb.Update(_container);

		protected bool IsRepositoryRegistered<TRepository>() where TRepository : IRepository => IsRepositoryRegistered(typeof(TRepository));

		protected bool IsRepositoryRegistered(Type repositoryType) => _container.IsRegistered(repositoryType);

		public TRepository CustomRepository<TRepository>() where TRepository : IRepository => GetRepository<TRepository>();

		public IRepository<T> Repository<T>() where T : class, new() => GetRepository<IRepository<T>>();

		public IRepository<TSource, TDestination> Repository<TSource, TDestination>() where TSource : class, new() where TDestination : class, new() => GetRepository<IRepository<TSource, TDestination>>();

		public IListRepository<TSource, TDestination, TListDestination> Repository<TSource, TDestination, TListDestination>() where TSource : class, new() where TDestination : class, new() where TListDestination : class, new() => GetRepository<IListRepository<TSource, TDestination, TListDestination>>();

		public virtual IQueryable<T> Data<T>() where T : class, new() => Enumerable.Empty<T>().AsQueryable();

		public virtual void Dispose()
		{
			_container.Dispose();
		}
	}
}

