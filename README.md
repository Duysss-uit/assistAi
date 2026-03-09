# AssistAI

AssistAI is an ASP.NET Core Web API that serves backend endpoints and static frontend files from AssistAi.Api/wwwroot. This README explains setup, configuration, and common development tasks.

## Quick start

1. Install the .NET 10 SDK: https://dotnet.microsoft.com/download
2. Restore dependencies and run the API from the repository root:

```powershell
dotnet restore .\AssistAi.Api\AssistAi.Api.csproj
dotnet run --project .\AssistAi.Api\AssistAi.Api.csproj
```

The app serves API endpoints and static files; the default SPA entry is `wwwroot/index.html`.

## Configuration

Set configuration values in `AssistAi.Api/appsettings.json` or via environment variables:

- ConnectionStrings:DefaultConnection - SQLite connection string
- Jwt:Key - secret key used to sign JWT tokens (keep private)
- OpenRouter:ApiKey - API key for OpenRouter integration
- OpenRouter:BaseUrl - optional base URL for OpenRouter

Do not commit secrets to source control. Prefer environment variables for local development and production.

## Development

Build and run with live-reload:

```powershell
dotnet build .\AssistAi.Api\AssistAi.Api.csproj -v minimal
dotnet watch run --project .\AssistAi.Api\AssistAi.Api.csproj
```

Common commands:
- dotnet restore .\AssistAi.Api\AssistAi.Api.csproj
- dotnet build .\AssistAi.Api\AssistAi.Api.csproj -v minimal
- dotnet run --project .\AssistAi.Api\AssistAi.Api.csproj

## Project layout

- AssistAi.Api/Controllers/ — API controllers (AuthController, ChatController, etc.)
- AssistAi.Api/Services/ — business logic and integrations
- AssistAi.Api/Data/ — EF Core DbContext and migrations
- AssistAi.Api/Models/ — EF entities (User, UsageLog, Payment)
- AssistAi.Api/wwwroot/ — static frontend (HTML, CSS, JS)

## Notes

- The project uses EF Core with SQLite by default and JWT for auth.
- There is currently no separate test project; verify changes by building and manual testing.

## Contributing

- Make changes on feature branches and open a PR with a clear description and verification steps.
- Follow C# conventions: PascalCase for types/members and keep nullability warnings addressed.

## License

This repository does not include a license file. Add one if required for your project.
