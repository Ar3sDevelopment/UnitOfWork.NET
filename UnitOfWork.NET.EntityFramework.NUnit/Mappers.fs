namespace UnitOfWork.NET.EntityFramework.NUnit

open ClassBuilder.Classes
open UnitOfWork.NET.EntityFramework.NUnit.Data.Models

module Mappers =     
    type UserMapper() = 
        inherit DefaultMapper<UserDTO, User>()
        override __.CustomMap(source, destination) = 
            destination.Id <- source.Id
            destination.Login <- source.Login
            destination.Password <- source.Password
            destination
    
    type UserDTOMapper() = 
        inherit DefaultMapper<User, UserDTO>()
        override __.CustomMap(source, destination) = 
            destination.Id <- source.Id
            destination.Login <- source.Login
            destination.Password <- source.Password
            destination