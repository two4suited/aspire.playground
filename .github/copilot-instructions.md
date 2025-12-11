# .NET Aspire Playground - AI Agent Instructions

## Project Overview
This is a .NET Aspire 13.0 distributed application using .NET 10.0. The solution follows Aspire's standard architecture pattern with an AppHost for orchestration and shared ServiceDefaults for common service configurations.

## Architecture

### Core Components
- **AspireApp.AppHost** (`AspireApp.AppHost/AppHost.cs`): Orchestration layer using `DistributedApplication.CreateBuilder(args)` pattern. This is where you define the app model - add resources (projects, containers, databases) and wire up dependencies using `.WithReference()`.
- **AspireApp.ServiceDefaults** (`AspireApp.ServiceDefaults/Extensions.cs`): Shared library providing `AddServiceDefaults<TBuilder>()` extension method that configures:
  - OpenTelemetry (logging, metrics, tracing with OTLP export)
  - Service discovery with HTTP client defaults
  - Standard resilience handlers (retry, circuit breaker, timeout)
  - Health checks (`/health`, `/alive` endpoints in development only)

### Key Patterns
- Services reference `AspireApp.ServiceDefaults` and call `.AddServiceDefaults()` in their `Program.cs`
- AppHost uses `builder.AddProject<Projects.ServiceName>()` to add .NET projects
- Use `.WithReference()` to inject connection strings and service endpoints between resources
- SDK-style projects: AppHost uses `Aspire.AppHost.Sdk/13.0.0`, services use standard .NET SDK with ServiceDefaults reference

## Developer Workflows

### Running the Application
```bash
aspire run
```
This launches the Aspire dashboard (typically http://localhost:15000) for monitoring services, logs, traces, and distributed tracing.

Alternative: `dotnet run --project AspireApp.AppHost`

### Updating Packages
```bash
# Update Aspire-specific packages
aspire update

# Check for outdated NuGet packages
dotnet tool install -g dotnet-outdated-tool  # First time only
dotnet outdated
```

### Adding New Services
1. Create service project: `dotnet new webapi -n AspireApp.ApiService`
2. Add to solution: `dotnet sln add AspireApp.ApiService`
3. Reference ServiceDefaults: `dotnet add AspireApp.ApiService reference AspireApp.ServiceDefaults`
4. In service's `Program.cs`: `builder.AddServiceDefaults()`
5. In AppHost's `AppHost.cs`: `builder.AddProject<Projects.AspireApp_ApiService>("apiservice")`

### Adding Resources (Redis, PostgreSQL, etc.)
Use Aspire hosting packages in AppHost:
```csharp
var cache = builder.AddRedis("cache");
var db = builder.AddPostgres("postgres").AddDatabase("mydb");
builder.AddProject<Projects.ApiService>("api")
    .WithReference(cache)
    .WithReference(db);
```

## Project-Specific Conventions

### Naming
- Projects follow `AspireApp.<ComponentName>` pattern
- Resource names in AppHost use lowercase (e.g., `"cache"`, `"apiservice"`)
- Project references in AppHost use `Projects.AspireApp_<Name>` format (underscores, not dots)

### Configuration
- AppHost uses User Secrets (ID: `46906e6e-eb0b-4a97-b422-d9a0e0eedcb7`) for sensitive config
- Health checks only exposed in Development environment for security
- OTLP exporter conditionally enabled via `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable

### Service Defaults Behavior
- HTTP clients automatically get resilience handlers and service discovery
- Tracing excludes `/health` and `/alive` endpoints to reduce noise
- "live" tagged health checks determine liveness; all checks determine readiness

## Technology Stack
- .NET 10.0
- Aspire SDK 13.0
- OpenTelemetry 1.13.x
- Microsoft.Extensions.ServiceDiscovery 10.0.0
- Microsoft.Extensions.Http.Resilience 10.0.0

## Important Notes
- Always use `AddServiceDefaults()` in new services to get observability and resilience
- Don't manually configure service discovery URLs - Aspire injects them via `.WithReference()`
- To enable Azure Monitor: uncomment code in `Extensions.cs` and set `APPLICATIONINSIGHTS_CONNECTION_STRING`
- To enable gRPC tracing: uncomment line in `ConfigureOpenTelemetry()` and add package reference
- For up-to-date Aspire documentation and examples, use Context7 with library ID `/dotnet/docs-aspire` or `/websites/aspire_dev`
