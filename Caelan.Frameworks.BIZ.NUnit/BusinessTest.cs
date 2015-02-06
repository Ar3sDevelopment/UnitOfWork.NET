using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			using (var uow = UnitOfWorkCaller.Context<TestDbContext>())
			{
				var users = uow.Repository<IEnumerable<User>, User>(t => t.All());

				foreach (var user in users)
				{
					Console.WriteLine("{0} {1}", user.Id, user.Login);
				}
			}
			stopWatch.Stop();
			Console.WriteLine("{0} ms", stopWatch.ElapsedMilliseconds);
		}

		[Test]
		public void TestRepository()
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			using (var uow = UnitOfWorkCaller.Context<TestDbContext>())
			{
				var users = uow.RepositoryList<User, UserDTO>();

				foreach (var user in users)
				{
					Console.WriteLine("{0} {1}", user.Id, user.Login);
				}
			}
			stopWatch.Stop();
			Console.WriteLine("{0} ms", stopWatch.ElapsedMilliseconds);
		}

		[Test]
		public void TestCustomRepository()
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			using (var uow = UnitOfWorkCaller.Context<TestDbContext>())
			{
				var users = uow.UnitOfWork(t => t.CustomRepository<UserRepository>().NewList());

				foreach (var user in users)
				{
					Console.WriteLine("{0} {1}", user.Id, user.Login);
				}
			}
			stopWatch.Stop();
			Console.WriteLine("{0} ms", stopWatch.ElapsedMilliseconds);
		}
	}
}

