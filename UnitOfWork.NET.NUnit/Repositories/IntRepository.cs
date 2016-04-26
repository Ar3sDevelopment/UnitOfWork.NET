using System;
using UnitOfWork.NET.Classes;
using UnitOfWork.NET.NUnit.Classes;
using UnitOfWork.NET.Interfaces;
namespace UnitOfWork.NET.NUnit.Repositories
{
	public class IntRepository : Repository<IntValue>
	{
		public IntRepository(IUnitOfWork manager) : base(manager)
		{
		}
	}
}

