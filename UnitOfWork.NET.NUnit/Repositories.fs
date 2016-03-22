namespace UnitOfWork.NET.NUnit

open UnitOfWork.NET.Classes

[<AutoOpen>]
module Repositories = 
    type DoubleRepository(manager) = 
        inherit Repository<DoubleValue, FloatValue>(manager)
        
        member this.NewList() = 
            "NewList" |> printfn "%s"
            this.AllBuilt()
    
    type IntRepository(manager) = 
        inherit Repository<IntValue>(manager)