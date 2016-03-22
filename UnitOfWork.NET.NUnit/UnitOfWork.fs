namespace UnitOfWork.NET.NUnit

open System
open UnitOfWork.NET.Classes

type TestUnitOfWork() = 
    inherit UnitOfWork()
    [<DefaultValue>]
    val mutable Ints : IntRepository
    [<DefaultValue>]
    val mutable Doubles : DoubleRepository

    override __.Data<'T>() =
        let dataType = typeof<'T>
        let rand = Random(int(DateTime.Now.Ticks))
        if dataType = typeof<int> then
            Seq.init(rand.Next() % 10 + 1) (fun t -> rand.Next()) :?> seq<'T>
        else if dataType = typeof<double> then
            Seq.init(rand.Next() % 10 + 1) (fun t -> double(rand.NextDouble())) :?> seq<'T>
        else if dataType = typeof<float> then
            Seq.init(rand.Next() % 10 + 1) (fun t -> rand.NextDouble()) :?> seq<'T>
        else base.Data<'T>()