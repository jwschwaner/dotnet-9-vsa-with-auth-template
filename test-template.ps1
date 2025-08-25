param()

$TemplatePath = $PSScriptRoot

function Cleanup-TestProjects {
    Write-Host "Cleaning up test projects..." -ForegroundColor Yellow
    if (Test-Path TestApi) { Remove-Item -Recurse -Force TestApi }
}

function Cleanup-And-Exit {
    param([int]$ExitCode = 1)
    Cleanup-TestProjects
    try {
    dotnet new uninstall "$TemplatePath" *>$null
    } catch { }
    Write-Host "Test failed. Cleaned up." -ForegroundColor Red
    exit $ExitCode
}

Write-Host "Testing .NET Web API VSA Template" -ForegroundColor Yellow

Write-Host "Cleaning up previous tests..." -ForegroundColor Yellow
Cleanup-TestProjects

Write-Host "Uninstalling any existing template..." -ForegroundColor Yellow
try { 
    dotnet new uninstall "$TemplatePath" *>$null
} catch { }

Write-Host "Installing template..." -ForegroundColor Yellow
dotnet new install "$TemplatePath" --force
if ($LASTEXITCODE -ne 0) {
    Write-Host "Template installation failed" -ForegroundColor Red
    Cleanup-And-Exit
}
Write-Host "Template installed successfully" -ForegroundColor Green

Write-Host "Creating project..." -ForegroundColor Yellow
dotnet new webapi-vsa -n TestApi --AdminEmail admin@test.com --AdminPassword Test123!
if ($LASTEXITCODE -ne 0) {
    Write-Host "Project creation failed" -ForegroundColor Red
    Cleanup-And-Exit
}
Write-Host "Project created successfully" -ForegroundColor Green

Write-Host "Building project..." -ForegroundColor Yellow
Push-Location TestApi/src/TestApi
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Project build failed" -ForegroundColor Red
    Pop-Location
    Cleanup-And-Exit
}
Write-Host "Project builds successfully" -ForegroundColor Green
Pop-Location

Write-Host "Checking file structure..." -ForegroundColor Yellow

if (Test-Path "TestApi/src/TestApi/Authentication") {
    Write-Host "Authentication folder exists" -ForegroundColor Green
} else {
    Write-Host "Authentication folder missing" -ForegroundColor Red
}

Write-Host "Template testing completed!" -ForegroundColor Green

$cleanup = Read-Host "Do you want to clean up test project? (y/n)"
if ($cleanup -eq "y" -or $cleanup -eq "Y") {
    Write-Host "Cleaning up..." -ForegroundColor Yellow
    if (Test-Path TestApi) { Remove-Item -Recurse -Force TestApi }
    try { dotnet new uninstall "$TemplatePath" *>$null } catch { }
    Write-Host "Cleanup completed" -ForegroundColor Green
}

Write-Host "Done. Press Enter to exit."
Read-Host