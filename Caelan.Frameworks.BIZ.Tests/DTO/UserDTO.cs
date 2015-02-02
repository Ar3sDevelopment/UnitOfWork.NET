using System.Collections.Generic;

namespace Caelan.Frameworks.BIZ.Tests.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public IEnumerable<UserRoleDTO> UserRoles { get; set; }
    }
}
