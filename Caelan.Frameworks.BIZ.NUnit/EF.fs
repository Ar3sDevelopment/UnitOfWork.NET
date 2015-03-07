namespace Caelan.Frameworks.BIZ.NUnit

open System
open System.Data.Entity
open System.Data.Entity.ModelConfiguration
open System.Collections.Generic

[<AutoOpen>]
module Entities =
    [<AllowNullLiteral>]
    type User() =
        member val Id = 0 with get, set
        member val Login = "" with get, set
        member val Password = "" with get, set
        [<DefaultValue>] val mutable UserRoles : ICollection<UserRole>

    and [<AllowNullLiteral>] Role() =
        member val Id = 0 with get, set
        member val Description = "" with get, set
        [<DefaultValue>] val mutable UserRoles : ICollection<UserRole>

    and [<AllowNullLiteral>] UserRole() =
        member val Id = 0 with get, set
        member val IdUser = 0 with get, set
        member val IdRole = 0 with get, set
        [<DefaultValue>] val mutable Role : Role
        [<DefaultValue>] val mutable User : User

[<AutoOpen>]
module EntityMappers =
    type UserMap() as this =
        inherit EntityTypeConfiguration<User>()
        do
            this.HasKey(fun t -> t.Id) |> ignore

            // Properties
            this.Property(fun t -> t.Login)
                .IsRequired()
                .HasMaxLength(Nullable<int>(50)) |> ignore

            this.Property(fun t -> t.Password)
                .IsRequired()
                .HasMaxLength(Nullable<int>(50)) |> ignore

            // Table & Column Mappings
            this.ToTable("User") |> ignore
            this.Property(fun t -> t.Id).HasColumnName("Id") |> ignore
            this.Property(fun t -> t.Login).HasColumnName("Login") |> ignore
            this.Property(fun t -> t.Password).HasColumnName("Password") |> ignore
     type RoleMap() as this =
         inherit EntityTypeConfiguration<Role>()
         do
            // Primary Key
            this.HasKey(fun t -> t.Id) |> ignore

            // Properties
            this.Property(fun t -> t.Description)
                .IsRequired()
                .HasMaxLength(Nullable<int>(50)) |> ignore

            // Table & Column Mappings
            this.ToTable("Role") |> ignore
            this.Property(fun t -> t.Id).HasColumnName("Id") |> ignore
            this.Property(fun t -> t.Description).HasColumnName("Description") |> ignore

      type UserRoleMap() as this =
          inherit EntityTypeConfiguration<UserRole>()
          do
              // Primary Key
            this.HasKey(fun t -> t.Id) |> ignore

            // Properties
            // Table & Column Mappings
            this.ToTable("UserRole") |> ignore
            this.Property(fun t -> t.Id).HasColumnName("Id") |> ignore
            this.Property(fun t -> t.IdUser).HasColumnName("IdUser") |> ignore
            this.Property(fun t -> t.IdRole).HasColumnName("IdRole") |> ignore

            // Relationships
            this.HasRequired(fun t -> t.Role)
                .WithMany(fun t -> t.UserRoles)
                .HasForeignKey(fun d -> d.IdRole) |> ignore
            this.HasRequired(fun t -> t.User)
                .WithMany(fun t -> t.UserRoles)
                .HasForeignKey(fun d -> d.IdUser) |> ignore

module Context =
    type TestDbContext() =
        inherit DbContext("Name=TestDbContext")
        static do Database.SetInitializer<TestDbContext>(null);

        [<DefaultValue>] val mutable Roles : DbSet<Role>
        [<DefaultValue>] val mutable Users : DbSet<User>
        [<DefaultValue>] val mutable UserRoles : DbSet<UserRole>

        override __.OnModelCreating modelBuilder =
            modelBuilder.Configurations.Add(UserRoleMap()) |> ignore
            modelBuilder.Configurations.Add(UserMap()) |> ignore
            modelBuilder.Configurations.Add(RoleMap()) |> ignore