namespace Caelan.Frameworks.BIZ.NUnit

open System
open System.Collections.Generic
open System.Diagnostics
open NUnit.Framework
open Caelan.Frameworks.Common.Classes
open Caelan.Frameworks.BIZ.Classes
open Caelan.Frameworks.BIZ.NUnit.Repositories
open Caelan.Frameworks.BIZ.NUnit.Context

[<TestFixture>]
type BusinessTest() = 
    [<Test>]
    member __.TestContext() =
        let stopwatch = Stopwatch()
        stopwatch.Start()

        use context = new TestDbContext()
        let users = context.Users

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

        uow.UnitOfWorkSaveChanges(fun t -> t.Repository<User>().Update(!entity)) |> ignore

        Assert.AreEqual ((!entity).Password, uow.UnitOfWork(fun t -> t.Repository<User>().SingleEntity((!entity).Id).Password))

        uow.UnitOfWorkSaveChanges(fun t -> t.Repository<User>().Delete(entity, (!entity).Id)) |> ignore

        Assert.IsNull (uow.UnitOfWork(fun t -> t.Repository<User>().SingleEntity((!entity).Id)))