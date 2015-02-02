namespace Caelan.Frameworks.BIZ.Tests.DTO
{
    public class UserRoleDTO
    {
        public int Id { get; set; }
        public int IdUser { get; set; }
        public int IdRole { get; set; }
        public RoleDTO Role { get; set; }
        public UserDTO User { get; set; }
    }
}
