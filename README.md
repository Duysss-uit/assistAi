# AssistAI API

AssistAI is now a backend-focused project: an ASP.NET Core Web API with static web pages served from `wwwroot`.

## Project Layout

- `AssistAi.Api/Controllers/`: API endpoints (`AuthController`, `ChatController`, `ModelsController`)
- `AssistAi.Api/Services/`: business logic (`AuthService`, `ChatService`, `UsageService`)
- `AssistAi.Api/Data/`: EF Core DbContext
- `AssistAi.Api/Models/`: entity models
- `AssistAi.Api/Migrations/`: EF Core migrations
- `AssistAi.Api/wwwroot/`: frontend pages (`index.html`, `login.html`, CSS/JS)

## Prerequisites

1. Install [.NET 10 SDK](https://dotnet.microsoft.com/download)
2. Configure API settings in `AssistAi.Api/appsettings.json` or environment variables:
   - `ConnectionStrings:DefaultConnection`
   - `Jwt:Key`
   - `OpenRouter:ApiKey`

## Run Locally

```bash
dotnet restore .\AssistAi.Api\AssistAi.Api.csproj
dotnet run --project .\AssistAi.Api\AssistAi.Api.csproj
```

- API and static frontend are served by the same app.
- Default static entry page: `/` -> `wwwroot/index.html`.

## Development Commands

```bash
dotnet build .\AssistAi.Api\AssistAi.Api.csproj -v minimal
dotnet watch run --project .\AssistAi.Api\AssistAi.Api.csproj
```

## Tech Stack

- C# / .NET 10
- ASP.NET Core Web API
- Entity Framework Core + SQLite
- JWT Authentication
- Static frontend (HTML/CSS/JS in `wwwroot`)
