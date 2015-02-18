using System;
using System.Collections.Generic;
using Caelan.Frameworks.BIZ.Classes;
using Caelan.Frameworks.BIZ.Interfaces;
using Caelan.Frameworks.BIZ.NUnit.DTO;
using Caelan.Frameworks.BIZ.NUnit.Models;

namespace Caelan.Frameworks.BIZ.NUnit.Repositories
{
	public class UserRepository : Repository<User, UserDTO>
	{
		public UserRepository(IUnitOfWork manager)
			: base(manager)
		{
		}

		public IEnumerable<UserDTO> NewList()
		{
			Console.WriteLine("NewList");
			return List();
		}
	}
}
