using System.Data.Entity.ModelConfiguration;

namespace Caelan.Frameworks.BIZ.NUnit.Data.Models.Mapping
{
    public class UserMap : EntityTypeConfiguration<User>
    {
        public UserMap()
        {
            // Primary Key
            HasKey(t => t.Id);

            // Properties
            Property(t => t.Login)
                .IsRequired()
                .HasMaxLength(50);

            Property(t => t.Password)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            ToTable("User");
            Property(t => t.Id).HasColumnName("Id");
            Property(t => t.Login).HasColumnName("Login");
            Property(t => t.Password).HasColumnName("Password");
        }
    }
}
