using System.Collections.Generic;

namespace Caelan.Frameworks.BIZ.Tests.DTO
{
    public class RoleDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public IEnumerable<UserRoleDTO> UserRoles { get; set; }
    }
}
