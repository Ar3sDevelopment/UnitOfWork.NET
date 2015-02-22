using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Caelan.Frameworks.BIZ.Classes;
using Caelan.Frameworks.BIZ.NUnit.DTO;
using Caelan.Frameworks.BIZ.NUnit.Models;
using Caelan.Frameworks.BIZ.NUnit.Repositories;
using Caelan.Frameworks.Common.Helpers;

namespace Caelan.Frameworks.BIZ.NUnit
{
	[TestFixture]
	public class BusinessTest
	{
		[Test]
		public void TestContext()
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			using (var db = new TestDbContext())
			{
				var users = db.Users;

				foreach (var user in users)
				{
					Console.WriteLine("{0} {1}", user.Id, user.Login);
				}
			}
			stopWatch.Stop();
			Console.WriteLine("{0} ms", stopWatch.ElapsedMilliseconds);
		}

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

				var entity = new User {
					Login = "test",
					Password = "test"
				};

				uow.UnitOfWorkSaveChanges(t => entity = t.Repository<User>().Insert(entity));

				Console.WriteLine(entity.Id);

				entity.Password = "test2";

				uow.UnitOfWorkSaveChanges(t => t.Repository<User>().Update(entity, entity.Id));
				uow.UnitOfWorkSaveChanges(t => t.Repository<User>().Delete(entity, entity.Id));
			}
			stopWatch.Stop();
			Console.WriteLine("{0} ms", stopWatch.ElapsedMilliseconds);
		}

		[Test]
		public void TestDTORepository()
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

				var dto = new UserDTO {
					Login = "test",
					Password = "test"
				};

				uow.UnitOfWorkSaveChanges(t => dto = t.Repository<User, UserDTO>().Insert(dto));

				dto = uow.UnitOfWork(t => t.Repository<User, UserDTO>().SingleDTO(d => d.Login == dto.Login));

				Console.WriteLine(dto.Id);

				dto.Password = "test2";

				uow.UnitOfWorkSaveChanges(t => t.Repository<User, UserDTO>().Update(dto, dto.Id));
				uow.UnitOfWorkSaveChanges(t => t.Repository<User, UserDTO>().Delete(dto, dto.Id));
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

				var dto = new UserDTO {
					Login = "test",
					Password = "test"
				};

				uow.UnitOfWorkSaveChanges(t => t.CustomRepository<UserRepository>().Insert(dto));

				dto = uow.UnitOfWork(t => t.CustomRepository<UserRepository>().SingleDTO(d => d.Login == dto.Login));

				Console.WriteLine(dto.Id);

				dto.Password = "test2";

				uow.UnitOfWorkSaveChanges(t => t.CustomRepository<UserRepository>().Update(dto, dto.Id));
				uow.UnitOfWorkSaveChanges(t => t.CustomRepository<UserRepository>().Delete(dto, dto.Id));
			}
			stopWatch.Stop();
			Console.WriteLine("{0} ms", stopWatch.ElapsedMilliseconds);
		}
	}
}

