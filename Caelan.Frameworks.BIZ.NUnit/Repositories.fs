namespace Caelan.Frameworks.BIZ.NUnit

open Caelan.Frameworks.BIZ.Classes

module Repositories =
    type UserRepository(manager) =
        inherit Repository<User, UserDTO>(manager)

        member this.NewList() =
            "NewList" |> printfn "%s"
            this.List()