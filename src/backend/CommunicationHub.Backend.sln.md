# Backend Solution – Skelett-Hinweis

> Diese Datei ist **kein** Visual-Studio-Solution-File. Sie markiert nur, dass an
> dieser Stelle in **Sprint 1** eine echte `CommunicationHub.Backend.sln` mit
> den folgenden Projekten angelegt wird.

## TODO Sprint 1

- `src/CommunicationHub.Backend.Api/CommunicationHub.Backend.Api.csproj` – ASP.NET Core 8 Web API
- `src/CommunicationHub.Backend.Core/CommunicationHub.Backend.Core.csproj` – Class Library (Domain, Orchestrierung)
- `tests/CommunicationHub.Backend.Tests/CommunicationHub.Backend.Tests.csproj` – xUnit + FluentAssertions + Testcontainers

Anschließend:

```powershell
dotnet new sln -n CommunicationHub.Backend
dotnet sln add src/CommunicationHub.Backend.Api/CommunicationHub.Backend.Api.csproj
dotnet sln add src/CommunicationHub.Backend.Core/CommunicationHub.Backend.Core.csproj
dotnet sln add tests/CommunicationHub.Backend.Tests/CommunicationHub.Backend.Tests.csproj
```

Diese Datei kann entfernt werden, sobald `.sln` existiert.
