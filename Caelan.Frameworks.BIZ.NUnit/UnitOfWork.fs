namespace Caelan.Frameworks.BIZ.NUnit

open Caelan.Frameworks.BIZ.Classes
open Caelan.Frameworks.BIZ.NUnit.Data.Models

type TestUnitOfWork() = 
    inherit UnitOfWork<TestDbContext>()
    [<DefaultValue>]
    val mutable Users : UserRepository
