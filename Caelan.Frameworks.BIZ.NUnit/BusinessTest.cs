using NUnit.Framework;
using System;
using System.Collections.Generic;
using Caelan.Frameworks.BIZ.Classes;
using Caelan.Frameworks.BIZ.NUnit.DTO;
using Caelan.Frameworks.BIZ.NUnit.Models;
using Caelan.Frameworks.BIZ.NUnit.Repositories;

namespace Caelan.Frameworks.BIZ.NUnit
{
	[TestFixture]
	public class BusinessTest
	{
		[Test]
		public void TestEntityRepository()
		{
			using (var uow = UnitOfWorkCaller.Context<TestDbContext>())
			{
				var users = uow.Repository<IEnumerable<User>, User>(t => t.All());

				foreach (var user in users)
				{
					Console.WriteLine("{0} {1}", user.Id, user.Login);
				}
			}
		}

		[Test]
		public void TestRepository()
		{
			using (var uow = UnitOfWorkCaller.Context<TestDbContext>())
			{
				var users = uow.RepositoryList<User, UserDTO>();

				foreach (var user in users)
				{
					Console.WriteLine("{0} {1}", user.Id, user.Login);
				}
			}
		}

		[Test]
		public void TestCustomRepository()
		{
			using (var uow = UnitOfWorkCaller.Context<TestDbContext>())
			{
				var users = uow.UnitOfWork(t => t.CustomRepository<UserRepository>().NewList());

				foreach (var user in users)
				{
					Console.WriteLine("{0} {1}", user.Id, user.Login);
				}
			}
		}

		[Test]
		public void TestTypes()
		{
			Console.WriteLine(typeof(List<>).GetGenericArguments().Length);
			Console.WriteLine(typeof(Dictionary<,>).GetGenericArguments().Length);
		}
	}
}

