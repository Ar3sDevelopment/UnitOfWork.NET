namespace Caelan.Frameworks.BIZ.NUnit

open System.Diagnostics
open NUnit.Framework
open Caelan.Frameworks.BIZ.Classes
open Caelan.Frameworks.BIZ.NUnit.Data.Models

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

        use uow = UnitOfWorkCaller.Context<TestDbContext>()

        let users = uow.Repository<seq<User>, User>(fun t -> t.All() :> seq<User>)

        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"

        for user in users do
            (user.Id, user.Login) ||> printfn "%d %s"

        let entity = ref(User(Login = "test", Password = "test"))

        uow.UnitOfWorkSaveChanges(fun t -> entity := t.Repository<User>().Insert(!entity)) |> ignore

        Assert.AreNotEqual(decimal((!entity).Id), 0m)

        (!entity).Id |> printfn "%d"

        (!entity).Password <- "test2"

        uow.UnitOfWorkSaveChanges(fun t -> t.Repository<User>().Update(!entity, (!entity).Id)) |> ignore

        Assert.AreEqual ((!entity).Password, uow.UnitOfWork(fun t -> t.Repository<User>().SingleEntity((!entity).Id).Password))

        uow.UnitOfWorkSaveChanges(fun t -> t.Repository<User>().Delete((!entity).Id)) |> ignore

        Assert.IsNull (uow.UnitOfWork(fun t -> t.Repository<User>().SingleEntity((!entity).Id)))

    [<Test>]
    member __.TestDTORepository() =
        let stopwatch = Stopwatch()
        stopwatch.Start()

        use uow = UnitOfWorkCaller.Context<TestDbContext>()

        let users = uow.RepositoryList<User, UserDTO>()

        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"

        for user in users do
            (user.Id, user.Login) ||> printfn "%d %s"

        let dto = ref(UserDTO(Login = "test", Password = "test"))

        uow.UnitOfWorkSaveChanges(fun t -> dto := t.Repository<User, UserDTO>().Insert(!dto)) |> ignore

        dto := uow.UnitOfWork(fun t ->
            let login = (!dto).Login
            t.Repository<User, UserDTO>().SingleDTO(fun d -> d.Login = login)
        )

        Assert.IsNotNull(!dto)

        Assert.AreNotEqual(decimal((!dto).Id), 0m)

        (!dto).Id |> printfn "%d"

        (!dto).Password <- "test2"

        uow.UnitOfWorkSaveChanges(fun t -> t.Repository<User, UserDTO>().Update(!dto, (!dto).Id)) |> ignore

        Assert.AreEqual ((!dto).Password, uow.UnitOfWork(fun t -> t.Repository<User, UserDTO>().SingleDTO((!dto).Id).Password))

        uow.UnitOfWorkSaveChanges(fun t -> t.Repository<User, UserDTO>().Delete((!dto).Id)) |> ignore

        Assert.IsNull (uow.UnitOfWork(fun t -> t.Repository<User, UserDTO>().SingleDTO((!dto).Id)))

    [<Test>]
    member __.TestCustomRepository() =
        let stopwatch = Stopwatch()
        stopwatch.Start()

        use uow = UnitOfWorkCaller.Context<TestDbContext>()

        let users = uow.UnitOfWork(fun t -> t.CustomRepository<UserRepository>().NewList())

        stopwatch.Stop()
        stopwatch.ElapsedMilliseconds |> printfn "%dms"

        for user in users do
            (user.Id, user.Login) ||> printfn "%d %s"

        let dto = ref(UserDTO(Login = "test", Password = "test"))

        uow.UnitOfWorkSaveChanges(fun t -> dto := t.CustomRepository<UserRepository>().Insert(!dto)) |> ignore

        dto := uow.UnitOfWork(fun t ->
            let login = (!dto).Login
            t.CustomRepository<UserRepository>().SingleDTO(fun d -> d.Login = login)
        )

        Assert.IsNotNull(!dto)

        Assert.AreNotEqual(decimal((!dto).Id), 0m)

        (!dto).Id |> printfn "%d"

        (!dto).Password <- "test2"

        uow.UnitOfWorkSaveChanges(fun t -> t.CustomRepository<UserRepository>().Update(!dto, (!dto).Id)) |> ignore

        Assert.AreEqual ((!dto).Password, uow.UnitOfWork(fun t -> t.CustomRepository<UserRepository>().SingleDTO((!dto).Id).Password))

        uow.UnitOfWorkSaveChanges(fun t -> t.CustomRepository<UserRepository>().Delete((!dto).Id)) |> ignore

        Assert.IsNull (uow.UnitOfWork(fun t -> t.CustomRepository<UserRepository>().SingleDTO((!dto).Id)))