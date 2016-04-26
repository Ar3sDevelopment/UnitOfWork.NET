namespace UnitOfWork.NET.FS.NUnit

open NUnit.Framework
open System.Diagnostics
open UnitOfWork.NET.FS.Classes

[<TestFixture>]
type UnitOfWorkTest() = 
    [<Test>]
    member __.TestSingleRepository() = 
        let stopwatch = Stopwatch()
        stopwatch.Start()
        use uow = new TestUnitOfWork()
        let numbers = uow.Repository<IntValue>().All()
        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"
        for number in numbers do
            number.Value |> printfn "%d"
    
    [<Test>]
    member __.TestDoubleRepository() = 
        let stopwatch = Stopwatch()
        stopwatch.Start()
        use uow = new TestUnitOfWork()
        let numbers = uow.Repository<DoubleValue, FloatValue>().AllBuilt()
        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"
        for number in numbers do
            number.Value |> printfn "%g"
    
    [<Test>]
    member __.TestCustomRepository() = 
        let stopwatch = Stopwatch()
        stopwatch.Start()
        use uow = new TestUnitOfWork()
        let numbers = uow.CustomRepository<DoubleRepository>().NewList()
        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"
        for number in numbers do
            number.Value |> printfn "%g"
    
    [<Test>]
    member __.TestCustomUnitOfWork() = 
        let stopwatch = Stopwatch()
        stopwatch.Start()
        use uow = new TestUnitOfWork()
        let numbers = uow.Doubles.NewList()
        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"
        for number in numbers do
            number.Value |> printfn "%g"