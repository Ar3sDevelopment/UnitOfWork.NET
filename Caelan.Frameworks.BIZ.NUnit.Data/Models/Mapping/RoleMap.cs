using System.Data.Entity.ModelConfiguration;

namespace Caelan.Frameworks.BIZ.NUnit.Data.Models.Mapping
{
    public class RoleMap : EntityTypeConfiguration<Role>
    {
        public RoleMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            ToTable("Role");
            Property(t => t.Id).HasColumnName("Id");
            Property(t => t.Description).HasColumnName("Description");
        }
    }
}
