using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Caelan.Frameworks.BIZ.Classes;
using Caelan.Frameworks.BIZ.Tests.Models;

namespace Caelan.Frameworks.BIZ.Tests
{
	[TestClass]
	public class BusinessTests
	{
		[TestMethod]
		public void TestUnitOfWork()
		{
			using (var uow = new UnitOfWork<TestDbContext>())
			{
			}
		}
	}
}
