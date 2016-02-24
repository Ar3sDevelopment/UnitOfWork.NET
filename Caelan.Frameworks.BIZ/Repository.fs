namespace Caelan.Frameworks.BIZ.Classes

open Caelan.Frameworks.BIZ.Interfaces

[<AbstractClass>]
type Repository(manager) = 
    
    interface IRepository with
        member this.UnitOfWork = this.UnitOfWork
    
    member val UnitOfWork = manager