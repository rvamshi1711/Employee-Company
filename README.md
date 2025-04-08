# BlazorTemplate# BlazorTemplate

## Overview
BlazorTemplate is a [Blazor](https://docs.microsoft.com/en-us/aspnet/core/blazor/) project that demonstrates the use of Blazor components and Entity Framework Core for building interactive web applications. The solution consists of multiple projects, each serving a specific purpose.

This project mainly consists of a Code First Entity Framework Core database and a Blazor front end. This project is setup to use [Interactive Server](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes) rendering where needed to enable interactive web pages.
When adding new pages, be sure to include `@rendermode InteractiveServer` at the top of the `.razor` file to enable interactive rendering.

## Build Instructions

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Not required, but recommended] [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with the following workloads:
  - ASP.NET and web development
  - .NET Core cross-platform development

### Steps to Build

#### [Recommended] Visual Studio 
The [Community Edition](https://visualstudio.microsoft.com/vs/community/) is free to use
1. **Clone the Repository** 

```
git clone https://github.com/WorkFirstCasualty/BlazorTemplate.git
```

2. **Open the Solution in Visual Studio**
3. Make sure that `BlazorTemplate` is set as the startup project.
4. Click "Run" within Visual Studio to build and run the application. 

#### .NET CLI [Install Instructions](https://learn.microsoft.com/en-us/dotnet/core/tools/)
1. **Clone the Repository**

```
git clone https://github.com/WorkFirstCasualty/BlazorTemplate.git
```

2. **Navigate to the BlazorTemplate directory**

```
cd BlazorTemplate
```

3. **To run the application in development mode**
 
```
dotnet watch --project BlazorTemplate
```

## Projects

### BlazorTemplate
This is the main Blazor project that contains the Blazor components and pages. It references the `BlazorTemplate.Data.Migrations` project for database migrations.

### BlazorTemplate.Data
This project contains the Entity Framework Core data models and the DbContext for the application.

[Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)

### BlazorTemplate.Data.Migrations
This project contains the Entity Framework Core (EF) migrations for the application. It references the `BlazorTemplate.Data` project.
To add a migration open a command prompt and navigate to the `BlazorTemplate.Data.Migrations` project directory. Then run the following command in the terminal: (requires [.NET Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet))
```
dotnet ef migrations add <MigrationName>
````
More information about migrations can be found [here](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations).

## Blazor Pages
- [Home.razor](https://github.com/WorkFirstCasualty/BlazorTemplate/blob/main/BlazorTemplate/Pages/Home.razor) and [Home.razor.cs](https://github.com/WorkFirstCasualty/BlazorTemplate/blob/main/BlazorTemplate/Pages/Home.razor.cs)
contain the logic for the home page. This page displays a list of workers in the database and allows the user to delete and add new workers.
- [AddWorker.razor](https://github.com/WorkFirstCasualty/BlazorTemplate/blob/main/BlazorTemplate/Pages/AddWorker.razor) and [AddWorker.razor.cs](https://github.com/WorkFirstCasualty/BlazorTemplate/blob/main/BlazorTemplate/Pages/AddWorker.razor.cs)
contain the logic and form for the user to add a new worker. This page uses Blazor's built in [EditForm](https://learn.microsoft.com/en-us/aspnet/core/blazor/forms) (found about a quarter the way down) to handle form submission and validation.
- [Companies.razor](https://github.com/WorkFirstCasualty/BlazorTemplate/blob/main/BlazorTemplate/Pages/Companies.razor) and [Companies.razor.cs](https://github.com/WorkFirstCasualty/BlazorTemplate/blob/main/BlazorTemplate/Pages/Companies.razor.cs)
are yet to be implemented.

## Gotchas
- Ensure that you have the correct Blazor Rendermode setup when testing a page. If buttons, forms, datagrids, or anything else that requires interactivity is not working, the first thing to check is if the page/component is setup to use `@rendermode InteractiveServer`.
- When changing/updating the database scheme in the [Entities](https://github.com/WorkFirstCasualty/BlazorTemplate/tree/main/BlazorTemplate.Data/Entities) folder. You must then create a new migration and update the database. 
Once you start the application the database will automatically be updated to the latest migration. This can be done by opening a terminal/command prompt and navigating to the `BlazorTemplate.Data.Migrations` 
project directory. Then run the following command: (requires [.NET Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)) 
```
dotnet ef migrations add <MigrationName>
```
- When quering the database using [LINQ](https://learn.microsoft.com/en-us/dotnet/csharp/linq) relational entities are not automatically populated. For example:
```csharp
var workers = db.Workers.ToList()
```
Will only contain a list of Worker entites and the `AssociatedCompany` property will be null. To include the `AssociatedCompany` property in the query you must use the `Include` method:
```csharp
var workers = db.Workers.Include(w => w.AssociatedCompany).ToList()
```
- When calling the above code, Entity Framework will track changies to each Worker Entity. Meaning if you change a property on a Worker Entity and then call `db.SaveChanges()` the changes will be saved to the database.
If you do not want this behavior by default, you must add `NoTracking` to your LINQ statement:
```csharp
var workers = db.Workers.AsNoTracking().ToList()
```
## Useful Links
- [Blazor documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [Information about Blazor render modes](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes)
- [Entity Framework Core documentation](https://docs.microsoft.com/en-us/ef/core/)
- [EF Migrations documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations)
- [.NET Core CLI installation instructions](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
- [LINQ Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/linq)
