namespace UnitOfWork.NET.EntityFramework.NUnit

open UnitOfWork.NET.EntityFramework.Classes
open UnitOfWork.NET.EntityFramework.NUnit.Data.Models

type TestUnitOfWork() = 
    inherit EntityUnitOfWork<TestDbContext>()
    [<DefaultValue>]
    val mutable Users : UserRepository
