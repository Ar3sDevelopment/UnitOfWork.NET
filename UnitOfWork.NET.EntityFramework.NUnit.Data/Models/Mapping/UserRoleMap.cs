using System.Data.Entity.ModelConfiguration;

namespace UnitOfWork.NET.EntityFramework.NUnit.Data.Models.Mapping
{
    public class UserRoleMap : EntityTypeConfiguration<UserRole>
    {
        public UserRoleMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            ToTable("UserRole");
            Property(t => t.Id).HasColumnName("Id");
            Property(t => t.IdUser).HasColumnName("IdUser");
            Property(t => t.IdRole).HasColumnName("IdRole");

            // Relationships
            HasRequired(t => t.Role)
                .WithMany(t => t.UserRoles)
                .HasForeignKey(d => d.IdRole);
            HasRequired(t => t.User)
                .WithMany(t => t.UserRoles)
                .HasForeignKey(d => d.IdUser);

        }
    }
}
