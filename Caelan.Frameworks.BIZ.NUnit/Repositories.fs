namespace Caelan.Frameworks.BIZ.NUnit

open Caelan.Frameworks.BIZ.Classes
open Caelan.Frameworks.BIZ.NUnit.Data.Models

[<AutoOpen>]
module Repositories =
    type UserRepository(manager) =
        inherit Repository<User, UserDTO>(manager)

        member this.NewList() =
            "NewList" |> printfn "%s"
            this.List()

    type RoleRepository(manager) =
        inherit Repository<Role>(manager)