namespace UnitOfWork.NET.NUnit

open UnitOfWork.NET.Classes
open UnitOfWork.NET.NUnit.Data.Models

[<AutoOpen>]
module Repositories = 
    type UserRepository(manager) = 
        inherit Repository<User, UserDTO>(manager)
        
        member this.NewList() = 
            "NewList" |> printfn "%s"
            this.AllBuilt()
    
    type RoleRepository(manager) = 
        inherit Repository<Role>(manager)