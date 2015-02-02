using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caelan.Frameworks.BIZ.Classes;
using Caelan.Frameworks.BIZ.Interfaces;
using Caelan.Frameworks.BIZ.Tests.DTO;
using Caelan.Frameworks.BIZ.Tests.Models;

namespace Caelan.Frameworks.BIZ.Tests.Repositories
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
