namespace UnitOfWork.NET.NUnit

open System
open UnitOfWork.NET.Classes

type TestUnitOfWork() = 
    inherit UnitOfWork()
    [<DefaultValue>]
    val mutable Ints : IntRepository
    [<DefaultValue>]
    val mutable Doubles : DoubleRepository

    override __.Data<'T when 'T : not struct>() =
        let dataType = typeof<'T>
        let rand = Random(int(DateTime.Now.Ticks))
        let count = rand.Next() % 10 + 1
        if dataType = typeof<IntValue> then
            Seq.init(count) (fun t -> IntValue(Value = rand.Next())) :?> seq<'T>
        else if dataType = typeof<DoubleValue> then
            Seq.init(count) (fun t -> DoubleValue(Value = double(rand.NextDouble()))) :?> seq<'T>
        else if dataType = typeof<FloatValue> then
            Seq.init(count) (fun t -> FloatValue(Value = rand.NextDouble())) :?> seq<'T>
        else base.Data<'T>()