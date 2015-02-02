using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Caelan.Frameworks.BIZ.Classes;
using Caelan.Frameworks.BIZ.Interfaces;
using Caelan.Frameworks.BIZ.Tests.DTO;
using Caelan.Frameworks.BIZ.Tests.Models;
using Caelan.Frameworks.BIZ.Tests.Repositories;

namespace Caelan.Frameworks.BIZ.Tests
{
	[TestClass]
	public class BusinessTests
	{
		[TestMethod]
		public void TestUnitOfWork()
		{
			using (var uow = UnitOfWorkCaller.Context<TestDbContext>())
			{
				var users = uow.UnitOfWork(t => t.Repository<UserRepository>().NewList());

				foreach (var user in users)
				{
					Console.WriteLine("{0} {1}", user.Id, user.Login);
				}
			}
		}
	}
}
