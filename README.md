#Caelan.Frameworks.BIZ NuGet Package

##What is it?
This package provides some utilities for the business layer like repositories and units of work.

##`IRepository`
The `IRepository` interface is the base interface of Repositories used in this framework. It has two methods for getting the linked `IUnitOfWork`
###`IRepository<TEntity, TDTO>`
The `IRepository<TEntity, TDTO>` interface inherits from `IRepository` and has more methods than its base and they're business related like getting the entire collection of DTOs or entities or getting one by ID or other CRUD operations

Work In Progress...
