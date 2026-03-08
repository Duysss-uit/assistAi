# Repository Guidelines

## Project Structure & Module Organization
This repository contains one .NET 10 backend project: `AssistAi.Api/AssistAi.Api.csproj`.

- Endpoints: `AssistAi.Api/Controllers/`
- Business logic: `AssistAi.Api/Services/`
- Data layer: `AssistAi.Api/Data/`, `AssistAi.Api/Models/`, `AssistAi.Api/Migrations/`
- Static frontend assets: `AssistAi.Api/wwwroot/` (`index.html`, `login.html`, `css/`, `js/`)

Build outputs (`bin/`, `obj/`) are generated and should not be committed.

## Build, Test, and Development Commands
Run commands from the repository root:

- `dotnet restore .\AssistAi.Api\AssistAi.Api.csproj`  
  Restores API dependencies.
- `dotnet build .\AssistAi.Api\AssistAi.Api.csproj -v minimal`  
  Builds the API.
- `dotnet run --project .\AssistAi.Api\AssistAi.Api.csproj`  
  Runs the local API server.
- `dotnet watch run --project .\AssistAi.Api\AssistAi.Api.csproj`  
  API live-reload during development.

## Coding Style & Naming Conventions
- Use standard C# conventions: 4-space indentation, PascalCase for types/methods/properties, camelCase for locals/parameters.
- Keep nullable reference types enabled and address warnings from `dotnet build`.
- Prefer clear file naming by responsibility (`AuthController.cs`, `ChatService.cs`, etc.).
- Keep API routes, service methods, and DTO/entity naming consistent by feature.

## Testing Guidelines
There is currently no dedicated test project in this repository. Before opening a PR:

- Run the API build command and verify zero errors.
- Manually smoke-test changed API endpoints and UI pages served from `wwwroot`.
- If adding non-trivial logic, create a new test project (for example `AssistAi.Tests`) using xUnit.

## Commit & Pull Request Guidelines
Git history follows conventional-style prefixes (mostly `feat:`). Use:

- `feat:`, `fix:`, `refactor:`, `docs:`, `chore:`

PRs should include:

- A clear summary of behavior changes.
- Linked issue/task (if available).
- Verification steps (commands run, endpoints exercised).
- Screenshots or sample request/response payloads for API/UI-visible changes.

## Security & Configuration Tips
- Never commit secrets. Keep real values for `Jwt:Key`, `OpenRouter:ApiKey`, and production connection strings out of source control.
- Treat `appsettings.json` as defaults only; override sensitive values per environment.
