namespace UnitOfWork.NET.NUnit

[<AutoOpen>]
module Classes =
    [<AllowNullLiteral>]
    type IntValue() =
        member val Value = 0 with get, set

    and [<AllowNullLiteral>] DoubleValue() =
        member val Value = 0.0 with get, set

    and [<AllowNullLiteral>] FloatValue() =
        member val Value = float(0.0) with get, set