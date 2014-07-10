namespace Caelan.Frameworks.BIZ.Interfaces

open System
open System.Data.Entity
open Caelan.Frameworks.DAL.Interfaces

[<AllowNullLiteral>]
type IDTO = 
    interface
    end

[<AllowNullLiteral>]
type IDTO<'TKey when 'TKey :> System.IEquatable<'TKey>> = 
    inherit IDTO
    abstract ID : 'TKey with get, set

[<AllowNullLiteral>]
type IBaseRepository = 
    interface
    end
