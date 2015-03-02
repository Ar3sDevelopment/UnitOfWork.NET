using Caelan.Frameworks.BIZ.NUnit.DTO;
using Caelan.Frameworks.BIZ.NUnit.Models;
using Caelan.Frameworks.Common.Classes;

namespace Caelan.Frameworks.BIZ.NUnit.Mappers
{
	public class UserDTOMapper : DefaultMapper<User, UserDTO>
	{
		public override void Map(User source, UserDTO destination)
		{
			destination.Id = source.Id;
			destination.Login = source.Login;
			destination.Password = source.Password;
		}
	}
}
