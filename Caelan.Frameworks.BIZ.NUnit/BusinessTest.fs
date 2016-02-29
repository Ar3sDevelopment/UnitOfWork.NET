namespace Caelan.Frameworks.BIZ.NUnit

open Caelan.Frameworks.BIZ.Classes
open Caelan.Frameworks.BIZ.NUnit.Data.Models
open NUnit.Framework
open System.Diagnostics

[<TestFixture>]
type BusinessTest() = 
    
    [<Test>]
    member __.TestContext() = 
        let stopwatch = Stopwatch()
        stopwatch.Start()
        use db = new TestDbContext()
        let users = db.Users
        for user in users do
            (user.Id, user.Login) ||> printfn "%d %s"
        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"
    
    [<Test>]
    member __.TestEntityRepository() = 
        let stopwatch = Stopwatch()
        stopwatch.Start()
        use uow = new UnitOfWork<TestDbContext>()
        let users = uow.Repository<User>().All()
        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"
        for user in users do
            (user.Id, user.Login) ||> printfn "%d %s"
        let entity = ref <| User(Login = "test", Password = "test")
        entity := uow.Repository<User>().Insert <| !entity
        uow.SaveChanges() |> ignore
        ((!entity).Id |> decimal, 0m) |> Assert.AreNotEqual
        (!entity).Id |> printfn "%d"
        (!entity).Password <- "test2"
        uow.Repository<User>().Update(!entity, (!entity).Id) |> ignore
        uow.SaveChanges() |> ignore
        Assert.AreEqual((!entity).Password, uow.Repository<User>().SingleEntity((!entity).Id).Password)
        uow.Repository<User>().Delete((!entity).Id) |> ignore
        uow.SaveChanges() |> ignore
        Assert.IsNull(uow.Repository<User>().SingleEntity((!entity).Id))
    
    [<Test>]
    member __.TestDTORepository() = 
        let stopwatch = Stopwatch()
        stopwatch.Start()
        use uow = new UnitOfWork<TestDbContext>()
        let users = uow.Repository<User, UserDTO>().List()
        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"
        for user in users do
            (user.Id, user.Login) ||> printfn "%d %s"
        let dto = ref <| UserDTO(Login = "test", Password = "test")
        dto := uow.Repository<User, UserDTO>().Insert(!dto)
        uow.SaveChanges() |> ignore
        let login = (!dto).Login
        dto := uow.Repository<User, UserDTO>().SingleDTO(fun d -> d.Login = login)
        Assert.IsNotNull(!dto)
        Assert.AreNotEqual(decimal ((!dto).Id), 0m)
        (!dto).Id |> printfn "%d"
        (!dto).Password <- "test2"
        uow.Repository<User, UserDTO>().Update(!dto, (!dto).Id) |> ignore
        uow.SaveChanges() |> ignore
        Assert.AreEqual((!dto).Password, uow.Repository<User, UserDTO>().SingleDTO((!dto).Id).Password)
        uow.Repository<User, UserDTO>().Delete((!dto).Id) |> ignore
        uow.SaveChanges() |> ignore
        Assert.IsNull(uow.Repository<User, UserDTO>().SingleDTO((!dto).Id))
    
    [<Test>]
    member __.TestCustomRepository() = 
        let stopwatch = Stopwatch()
        stopwatch.Start()
        use uow = new UnitOfWork<TestDbContext>()
        let users = uow.CustomRepository<UserRepository>().NewList()
        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"
        for user in users do
            (user.Id, user.Login) ||> printfn "%d %s"
        let dto = ref <| UserDTO(Login = "test", Password = "test")
        dto := uow.CustomRepository<UserRepository>().Insert(!dto)
        uow.SaveChanges() |> ignore
        let login = (!dto).Login
        dto := uow.CustomRepository<UserRepository>().SingleDTO(fun d -> d.Login = login)
        Assert.IsNotNull(!dto)
        Assert.AreNotEqual(decimal ((!dto).Id), 0m)
        (!dto).Id |> printfn "%d"
        (!dto).Password <- "test2"
        uow.CustomRepository<UserRepository>().Update(!dto, (!dto).Id) |> ignore
        uow.SaveChanges() |> ignore
        Assert.AreEqual((!dto).Password, uow.CustomRepository<UserRepository>().SingleDTO((!dto).Id).Password)
        uow.CustomRepository<UserRepository>().Delete((!dto).Id) |> ignore
        uow.SaveChanges() |> ignore
        Assert.IsNull(uow.CustomRepository<UserRepository>().SingleDTO((!dto).Id))
    
    [<Test>]
    member __.TestCustomUnitOfWork() = 
        let stopwatch = Stopwatch()
        stopwatch.Start()
        use uow = new TestUnitOfWork()
        let users = uow.Users.NewList()
        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"
        for user in users do
            (user.Id, user.Login) ||> printfn "%d %s"
        let dto = ref <| UserDTO(Login = "test", Password = "test")
        dto := uow.Users.Insert(!dto)
        uow.SaveChanges() |> ignore
        let login = (!dto).Login
        dto := uow.Users.SingleDTO(fun d -> d.Login = login)
        Assert.IsNotNull(!dto)
        Assert.AreNotEqual(decimal ((!dto).Id), 0m)
        (!dto).Id |> printfn "%d"
        (!dto).Password <- "test2"
        uow.Users.Update(!dto, (!dto).Id) |> ignore
        uow.SaveChanges() |> ignore
        Assert.AreEqual((!dto).Password, uow.Users.SingleDTO((!dto).Id).Password)
        uow.Users.Delete((!dto).Id) |> ignore
        uow.SaveChanges() |> ignore
        Assert.IsNull(uow.Users.SingleDTO((!dto).Id))