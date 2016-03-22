namespace UnitOfWork.NET.NUnit

open NUnit.Framework
open System.Diagnostics
open UnitOfWork.NET.Classes

[<TestFixture>]
type UnitOfWorkTest() = 
    [<Test>]
    member __.TestSingleRepository() = 
        let stopwatch = Stopwatch()
        stopwatch.Start()
        use uow = new TestUnitOfWork()
        let numbers = uow.Repository<int>().All()
        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"
        for number in numbers do
            number |> printfn "%d"
    
    [<Test>]
    member __.TestDoubleRepository() = 
        let stopwatch = Stopwatch()
        stopwatch.Start()
        use uow = new TestUnitOfWork()
        let numbers = uow.Repository<double, float>().AllBuilt()
        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"
        for number in numbers do
            number |> printfn "%g"
    
    [<Test>]
    member __.TestCustomRepository() = 
        let stopwatch = Stopwatch()
        stopwatch.Start()
        use uow = new TestUnitOfWork()
        let numbers = uow.CustomRepository<DoubleRepository>().NewList()
        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"
        for number in numbers do
            number |> printfn "%g"
    
    [<Test>]
    member __.TestCustomUnitOfWork() = 
        let stopwatch = Stopwatch()
        stopwatch.Start()
        use uow = new TestUnitOfWork()
        let numbers = uow.Doubles.NewList()
        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"
        for number in numbers do
            number |> printfn "%g"