using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Caelan.Frameworks.BIZ.Classes;
using Caelan.Frameworks.BIZ.Tests.DTO;
using Caelan.Frameworks.BIZ.Tests.Models;
using Caelan.Frameworks.BIZ.Tests.Repositories;

namespace Caelan.Frameworks.BIZ.Tests
{
	[TestClass]
	public class BusinessTests
	{
		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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
	}
}
