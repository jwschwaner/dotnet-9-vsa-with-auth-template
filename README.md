# MyWebApi.VSA.Template

A .NET 9 Web API template using Vertical Slice Architecture, Entity Framework Core, Identity, Serilog, and Scalar for API docs.

## What's here
- `content/` – the template source that gets copied into new projects
- `.template.config/template.json` – template metadata (identity, shortName, parameters)
- `test-template.ps1` – quick script to install the template locally, create a sample app, and build it

## Quick start
1. Install the template locally
   - Windows PowerShell:
     - Run the script: `./test-template.ps1`
   - Or manually:
     - `dotnet new install . --force`
2. Create a project
   - `dotnet new webapi-vsa -n MyApi --AdminEmail admin@example.com --AdminPassword Passw0rd!`
3. Build and run
   - `cd MyApi/src/MyApi`
   - `dotnet build`
   - `dotnet run`

API docs are served by Scalar at `/` in Development.

## Uninstall
- `dotnet new uninstall .`

