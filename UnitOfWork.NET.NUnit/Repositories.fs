namespace UnitOfWork.NET.NUnit

open UnitOfWork.NET.Classes
open UnitOfWork.NET.NUnit.Data.Models

[<AutoOpen>]
module Repositories = 
    type UserRepository(manager) = 
        inherit Repository<User, UserDTO>(manager)
        
        member this.NewList() = 
            "NewList" |> printfn "%s"
            this.List()
        
        override this.OnSaveChanges usersByStates = 
            for state in usersByStates do
                for user in state.Value do
                    (user.Id, user.Login, state.Key.ToString()) |||> printfn "[%d] %s %s"
    
    type RoleRepository(manager) = 
        inherit Repository<Role>(manager)