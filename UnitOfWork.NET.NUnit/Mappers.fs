namespace UnitOfWork.NET.NUnit

open ClassBuilder.Classes

module Mappers = 
    open UnitOfWork.NET.NUnit.Data.Models
    
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