namespace Caelan.Frameworks.BIZ.NUnit

[<AutoOpen>]
module DTO =
    [<AllowNullLiteral>]
    type UserDTO() =
        member val Id = 0 with get, set
        member val Login = "" with get, set
        member val Password = "" with get, set
        member val UserRoles = Seq.empty<UserRoleDTO> with get, set

    and [<AllowNullLiteral>] RoleDTO() =
        member val Id = 0 with get, set
        member val Description = "" with get, set
        member val UserRoles = Seq.empty<UserRoleDTO> with get, set

    and [<AllowNullLiteral>] UserRoleDTO() =
        member val Id = 0 with get, set
        member val IdUser = 0 with get, set
        member val IdRole = 0 with get, set
        member val Role : RoleDTO = null with get, set
        member val User : UserDTO = null with get, set