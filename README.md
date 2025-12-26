ThikaResQNet - ASP.NET Core Web API

This project is a minimal clean-architecture sample with:
- Controllers, Models, DTOs, Services, Data, Repositories
- Entity Framework Core with SQL Server
- CORS enabled
- Swagger enabled

To run:
1. Update `appsettings.json` connection string if needed.
2. From the project folder run `dotnet ef migrations add Initial` then `dotnet ef database update` to create the database.
3. Run `dotnet run`.
