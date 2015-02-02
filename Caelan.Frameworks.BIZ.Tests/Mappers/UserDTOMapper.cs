using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caelan.Frameworks.BIZ.Tests.DTO;
using Caelan.Frameworks.BIZ.Tests.Models;
using Caelan.Frameworks.Common.Classes;

namespace Caelan.Frameworks.BIZ.Tests.Mappers
{
	public class UserDTOMapper : DefaultMapper<User, UserDTO>
	{
		public override void Map(User source, ref UserDTO destination)
		{
			destination.Id = source.Id;
			destination.Login = source.Login;
		}
	}
}
