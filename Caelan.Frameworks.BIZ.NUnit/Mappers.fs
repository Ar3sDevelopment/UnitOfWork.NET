namespace Caelan.Frameworks.BIZ.NUnit

open Caelan.Frameworks.Common.Classes

module Mappers =
    open Caelan.Frameworks.BIZ.NUnit.Data.Models

    type UserMapper() =
        inherit DefaultMapper<UserDTO, User>()

        override __.Map(source, destination) =
            destination.Id <- source.Id
            destination.Login <- source.Login
            destination.Password <- source.Password
            destination

    type UserDTOMapper() =
        inherit DefaultMapper<User, UserDTO>()

        override __.Map(source, destination) =
            destination.Id <- source.Id
            destination.Login <- source.Login
            destination.Password <- source.Password
            destination

