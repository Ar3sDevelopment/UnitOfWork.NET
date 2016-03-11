namespace UnitOfWork.NET.Classes

open UnitOfWork.NET.Interfaces

[<AbstractClass>]
type Repository(manager) = 
    
    interface IRepository with
        member this.UnitOfWork = this.UnitOfWork
    
    member val UnitOfWork = manager