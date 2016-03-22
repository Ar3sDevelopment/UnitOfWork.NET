namespace UnitOfWork.NET.EntityFramework.NUnit

open UnitOfWork.NET.Classes
open UnitOfWork.NET.EntityFramework.Classes
open UnitOfWork.NET.EntityFramework.NUnit.Data.Models

[<AutoOpen>]
module Repositories = 
    type UserRepository(manager) = 
        inherit EntityRepository<User, UserDTO>(manager)
        
        member this.NewList() = 
            "NewList" |> printfn "%s"
            this.List()
        
        override this.OnSaveChanges usersByStates = 
            for state in usersByStates do
                for user in state.Value do
                    (user.Id, user.Login, state.Key.ToString()) |||> printfn "[%d] %s %s"
    
    type RoleRepository(manager) = 
        inherit EntityRepository<Role>(manager)