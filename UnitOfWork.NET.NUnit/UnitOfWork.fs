namespace UnitOfWork.NET.NUnit

open UnitOfWork.NET.Classes
open UnitOfWork.NET.NUnit.Data.Models

type TestUnitOfWork() = 
    inherit UnitOfWork<TestDbContext>()
    [<DefaultValue>]
    val mutable Users : UserRepository
