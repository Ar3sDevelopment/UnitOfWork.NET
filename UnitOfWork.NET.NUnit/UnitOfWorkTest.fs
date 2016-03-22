namespace UnitOfWork.NET.NUnit

open NUnit.Framework
open System.Diagnostics
open UnitOfWork.NET.Classes
open UnitOfWork.NET.NUnit.Data.Models

[<TestFixture>]
type UnitOfWorkTest() = 
    [<Test>]
    member __.TestSingleRepository() = 
        let stopwatch = Stopwatch()
        stopwatch.Start()
        use uow = new UnitOfWork()
        let users = uow.Repository<User>().All()
        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"
        for user in users do
            (user.Id, user.Login) ||> printfn "%d %s"
    
    [<Test>]
    member __.TestDoubleRepository() = 
        let stopwatch = Stopwatch()
        stopwatch.Start()
        use uow = new UnitOfWork()
        let users = uow.Repository<User, UserDTO>().AllBuilt()
        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"
        for user in users do
            (user.Id, user.Login) ||> printfn "%d %s"
    
    [<Test>]
    member __.TestCustomRepository() = 
        let stopwatch = Stopwatch()
        stopwatch.Start()
        use uow = new UnitOfWork()
        let users = uow.CustomRepository<UserRepository>().NewList()
        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"
        for user in users do
            (user.Id, user.Login) ||> printfn "%d %s"
    
    [<Test>]
    member __.TestCustomUnitOfWork() = 
        let stopwatch = Stopwatch()
        stopwatch.Start()
        use uow = new TestUnitOfWork()
        let users = uow.Users.NewList()
        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"
        for user in users do
            (user.Id, user.Login) ||> printfn "%d %s"