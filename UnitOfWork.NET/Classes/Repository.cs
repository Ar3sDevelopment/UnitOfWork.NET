using System;
using UnitOfWork.NET.Interfaces;
namespace UnitOfWork.NET.Classes
{
	public class Repository : IRepository
	{
		public IUnitOfWork UnitOfWork { get; private set; }
		public Repository(IUnitOfWork manager)
		{
			UnitOfWork = manager;
		}
	}

	public class Repository<T> : Repository
	{
		public Repository(IUnitOfWork manager) : base(manager)
		{
		}
	}
	public class Repository<TSource, TDestination> : Repository<TSource>//, IRepository<TSource, TDestination> where TSource : class where TDestination : class
	{
		public Repository(IUnitOfWork manager) : base(manager)
		{
		}
	}
	public class ListRepository<TSource, TDestination, TListDestination> : Repository<TSource, TDestination>//, IListRepository<TSource, TDestination, TListDestination> where TSource : class where TDestination : class where TListDestination : class
	{
		public ListRepository(IUnitOfWork manager) : base(manager)
		{
		}
	}
}

