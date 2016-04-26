namespace UnitOfWork.NET.FS.Classes

open UnitOfWork.NET.FS.Interfaces

[<AbstractClass>]
type Repository(manager) = 
    
    interface IRepository with
        member this.UnitOfWork = this.UnitOfWork
    
    member val UnitOfWork = manager