namespace UnitOfWork.NET.NUnit

open System
open UnitOfWork.NET.Classes
open UnitOfWork.NET.NUnit.Data.Models

type TestUnitOfWork() = 
    inherit UnitOfWork()
    [<DefaultValue>]
    val mutable Users : UserRepository

    override __.Data<'T>() =
        let dataType = typeof<'T>
        let intType = typeof<int>
        let doubleType = typeof<double>
        let rand = Random(int(DateTime.Now.Ticks))
        if dataType = intType then
            Seq.init(rand.Next() % 10 + 1) (fun t -> rand.Next()) :?> seq<'T>
        else if dataType = doubleType then
            Seq.init(rand.Next() % 10 + 1) (fun t -> rand.NextDouble()) :?> seq<'T>
        else base.Data<'T>()