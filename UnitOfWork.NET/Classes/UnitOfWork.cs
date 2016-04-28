﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Autofac;
using UnitOfWork.NET.Interfaces;
using System.Linq;

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

            cb = new ContainerBuilder();

            var fields = GetType().GetFields().ToArray();
            var properties = GetType().GetProperties().ToArray();

            foreach (var type in fields.Select(t => t.FieldType).Union(properties.Select(t => t.PropertyType)).Where(IsRepository)) cb.RegisterType(type).AsSelf().AsImplementedInterfaces();

            cb.Update(_container);

            foreach (var field in fields.Where(t => t.FieldType.IsAssignableTo<IRepository>())) field.SetValue(this, _container.ResolveOptional(field.FieldType));
            foreach (var property in properties.Where(t => t.PropertyType.IsAssignableTo<IRepository>())) property.SetValue(this, _container.ResolveOptional(property.PropertyType));

            _assemblies.CollectionChanged += (sender, args) => RegisterAssembly(args.NewItems.Cast<Assembly>().ToArray());

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) _assemblies.Add(assembly);
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

            foreach (var assembly in assemblyArr.SelectMany(t => t.GetReferencedAssemblies()).Select(Assembly.Load).Where(
                t =>
                {
                    try
                    {
                        return !_assemblies.Contains(t) && t.GetTypes().Any(IsRepository);
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
                if (repositoryType.IsInterface || repositoryType.IsAbstract || _container.IsRegistered(repositoryType)) continue;

                if (repositoryType.IsGenericTypeDefinition)
                    cb.RegisterGeneric(repositoryType).AsSelf().AsImplementedInterfaces();
                else
                    cb.RegisterType(repositoryType).AsSelf().AsImplementedInterfaces();
            }

            cb.Update(_container);
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

        public TRepository CustomRepository<TRepository>() where TRepository : IRepository => GetRepository<TRepository>();

        public IRepository<T> Repository<T>() where T : class => GetRepository<IRepository<T>>();

        public IRepository<TSource, TDestination> Repository<TSource, TDestination>() where TSource : class where TDestination : class => GetRepository<IRepository<TSource, TDestination>>();

        public IListRepository<TSource, TDestination, TListDestination> Repository<TSource, TDestination, TListDestination>() where TSource : class where TDestination : class where TListDestination : class => GetRepository<IListRepository<TSource, TDestination, TListDestination>>();

        public virtual IEnumerable<T> Data<T>() where T : class => Enumerable.Empty<T>();

        public virtual void Dispose()
        {
            _container.Dispose();
        }
    }
}
