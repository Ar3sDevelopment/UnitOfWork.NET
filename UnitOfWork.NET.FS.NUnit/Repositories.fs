namespace UnitOfWork.NET.FS.NUnit

open UnitOfWork.NET.FS.Classes

[<AutoOpen>]
module Repositories = 
    type DoubleRepository(manager) = 
        inherit Repository<DoubleValue, FloatValue>(manager)
        
        member this.NewList() = 
            "NewList" |> printfn "%s"
            this.AllBuilt()
    
    type IntRepository(manager) = 
        inherit Repository<IntValue>(manager)