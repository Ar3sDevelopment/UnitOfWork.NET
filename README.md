#Business Framework
The Caelan.Frameworks.BIZ NuGet Package
If you need support or you want to contact me I'm [CaelanIt](https://twitter.com/CaelanIt) on Twitter

##What is it?
This package provides some utilities for the business layer like repositories and units of work.

##`IRepository`
The `IRepository` interface is the base interface of Repositories used in this framework. It has two methods for getting the linked `IUnitOfWork` instance.
`IRepository` has two methods because you can get a generic `IUnitOfWork` or a custom `UnitOfWork` type.

###`Repository`
The `Repository` class implements `IRepository` interface and has a static method for fluent notation for getting a `Repository<TEntity, TDTO>` specifying only `TEntity` and `TDTO`
This class is abstract because I thought that is useless for other uses.

###`IRepository<TEntity>`
The `IRepository<TEntity>` interface inherits from `IRepository` and has more methods than its base and they're business related like basic CRUD operations.

###`IRepository<TEntity, TDTO>`
The `IRepository<TEntity, TDTO>` interface inherits from `IRepository<TEntity>` and has more methods than its base and they're business related like basic CRUD operations.
`IRepository<TEntity, TDTO>` has also a method for return paginated result.

####`Repository<TEntity, TDTO>`
The `Repository<TEntity, TDTO>` inherits from `Repository` and implements `IRepository<TEntity, TDTO>` interface with some `EntityFramework` code for doing CRUD operations.
If you need some custom methods you can inherit from `Repository<TEntity, TDTO>` and use all the business ready methods.
Supposing you need a method to get if a user is admin:
```csharp
//Custom `Repository` implemetation class
public bool IsAdmin(int id)
{
  var user = SingleEntity(id);
  return user != null ? user.Admin : false;
}
```
Or if you need a method that return all admin users:
```csharp
//Custom `Repository` implemetation class
public IEnumerable<UserDTO> AllAdmins()
{
  return List(t => t.Admin); //DTOs
}

public IEnumerable<User> AllAdminsEntities()
{
  return All(t => t.Admin);
}

//using builders from my Common Framework
public IEnumerable<User> AllAdminsEntities()
{
  return DTOBuilder().BuildList(AllAdminsEntities());
}
```
Last solution is preferred because you can methods that returns subset of records and when you need DTOs you can simply build them with provided `Builder`s methods: `DTOBuilder()` for Entity to DTO mappings and `EntityBuilder()` for DTO to Entity mappings.
They use `Builder` class from my Common framework.

The paginated result uses method All with pagination parameters. It returns a `DataSourceResult`, that can also be used in WCF, with the page record and the total. `Sort` and `Filter` classes are in my DynamicLinq framework and contains data for sorting and filtering data for the page you need.

####Accessing to other `Repository` from current
If you need to use another `Repository` while doing operation in a `Repository` you can use the `UnitOfWork` method for going to container and access to other repositories like explained later.
Here an example:
```csharp
public bool CheckLogin(string username, string password)
{
  var user = Single(t => t.Username == username && t.Password == passowrd); //not secure!!
  var res = user != null;

  if (res)
  {
    var access = new UserAccessDTO { IDUser = user.ID, Date = DateTime.Now };
    UnitOfWork().Repository<UserAccess, UserAccessDTO>().Insert(access);
  }

  return res;
}
```
With this code you can log a user access without leaving `UserRepository` class and delegate this to the caller method.
Remember to keep `Repository` methods simple and without doing action out of their scope.

####`IListRepository<TEntity, TDTO, TListDTO>`
The `IListRepository<TEntity, TDTO, TListDTO>` is an interface that inherits from `IRepository<TEntity, TDTO>` and has only one get-set property for another `IRepository` but for `TEntity` and `TListDTO` so you can have two DTO for the same type, one lighter for reading lists of DTOs and one complete for CRUD operations.
I use it when I have to return data using WCF and data must navigate deep inside objects.

##`IUnitOfWork`
The `IUnitOfWork` interface contains base methods for the `UnitOfWork` like transactions, save changes and getting repositories.

###`UnitOfWork`
There are two `UnitOfWork` classes, one wants `DbContext` in constructor, the other inherits from this and wants only the `DbContext` type and instantiate it with default constructor.
`UnitOfWork` implements the `IDisposable` interface so you can use it in the `using` closure and it disposes the context only if it's instatiated by the `UnitOfWork` otherwise you will be responsible for it.
`UnitOfWork` class, like `Builder`, uses reflection for getting `IRepository` objects that are unknown. You can get the repository in two ways:
```csharp
using (var uow = new UnitOfWork<TestDbContext>()) //the uow object is responsible for disposing the context
{
  const int id = 1;
  var user = uow.Repository<User, UserDTO>().SingleDTO(id);
  if (user != null && uow.CustomRepository<UserRepository>().IsAdmin(user.Id))
  {
    //user is admin
  }
  else if (user != null)
  {
    //user exists but is not admin
  }
  else
  {
    //user not found
  }
}
```
The `user` object is retrieved by `IRepository<TEntity, TDTO>` exposed methods because the repository type is not known at compile time. We can check the admin status by retrieving the repository by type so we know it at compile time and we can use its business related methods like `IsAdmin`.

###`IUnitOfWorkCaller<TUnitOfWork>`
The `IUnitOfWorkCaller<TUnitOfWork>` interface contains methods for a class that performs lambda expressions on a `UnitOfWork`

####`UnitOfWorkCaller`
`UnitOfWorkCaller` is a static class for fluently accessing to a `IUnitOfWork`: one by `DbContext` and you get a simple `IUnitOfWork` or one by `IUnitOfWork` and you get all custom methods you have customized.

This class helps writing code because you have not to worry about creating and disposing `UnitOfWork` or `DbContext` objects.

Previous `UnitOfWork` code can be written like:
```csharp
UnitOfWorkCaller.Context<TestDbContext>().UnitOfWork(uow =>
{
  const int id = 1;
  var user = uow.Repository<User, UserDTO>().SingleDTO(id);
  if (user != null && uow.CustomRepository<UserRepository>().IsAdmin(user.Id))
  {
    //user is admin
  }
  else if (user != null)
  {
    //user exists but is not admin
  }
  else
  {
    //user not found
  }
});
```
If you need a return value from this lamda simply return it, it automatically returns it to the expression value.
