![Icon](https://github.com/asherber/StaTypPocoQueries.PetaPoco/raw/master/media/static-64.png)

# StaTypPocoQueries.PetaPoco [![NuGet](https://img.shields.io/nuget/v/StaTypPocoQueries.PetaPoco.svg)](https://nuget.org/packages/StaTypPocoQueries.PetaPoco)

[PetaPoco](https://github.com/CollaboratingPlatypus/PetaPoco) bindings for [StaTypPocoQueries](https://github.com/d-p-y/statically-typed-poco-queries), allowing you to use some simple, strongly typed LINQ expressions in your queries. 

`Database` extension methods are provided for `Query()`, `Fetch()`, `Page()`, `SkipTake()`, `Single()`, `SingleOrDefault()`, `First()`, `FirstOrDefault()`, and `Delete()`, essentially letting you use an expression in place of a hand-written `WHERE` clause. Column names are escaped using the `DatabaseProvider` for the `Database`.

Because StaTypPocoQueries includes support for F# quotations, bringing FSharp.Core along for the ride, this library supports those as well.

## Usage

These examples assume that `Database.EnableAutoSelect == true`, so that the `SELECT` (or `DELETE`) portion of the SQL command is generated for you.

```csharp
public class MyClass
{
    public int ID { get; set; }
    public string Name { get; set; }    
}

// Equivalent to db.Query<MyClass>("WHERE [ID] = @0", 4)
db.Query<MyClass>(c => c.ID == 4);

// Equivalent to db.Query<MyClass>("WHERE [ID] > @0", 8)
db.Query<MyClass>(c => c.ID > 8);

// Equivalent to db.Query<MyClass>("WHERE [Name] IS NULL")
db.Query<MyClass>(c => c.Name == null);

// Equivalent to db.Query<MyClass>("WHERE [ID] = @0 OR [ID] = @1", new [] { 1, 2 })
db.Query<MyClass>(c => c.ID == 1 || c.ID == 2);

// Equivalent to db.Query<MyClass>("WHERE [ID] = @0 AND [Name] = @1", new object[] { 10, "Bob" })
db.Query<MyClass>(c => c.ID == 10 && c.Name == "Bob");
```

