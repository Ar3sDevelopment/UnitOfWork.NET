namespace Caelan.Frameworks.BIZ.NUnit

open Caelan.Frameworks.BIZ.Classes
open Caelan.Frameworks.BIZ.NUnit.Data.Models

module Repositories =
    type UserRepository(manager) =
        inherit Repository<User, UserDTO>(manager)

        member this.NewList() =
            "NewList" |> printfn "%s"
            this.List()