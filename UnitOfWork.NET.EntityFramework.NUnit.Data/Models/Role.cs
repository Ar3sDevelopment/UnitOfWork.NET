using System.Collections.Generic;

namespace UnitOfWork.NET.EntityFramework.NUnit.Data.Models
{
    public partial class Role
    {
        public Role()
        {
            UserRoles = new List<UserRole>();
        }

        public int Id { get; set; }
        public string Description { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
