@echo off
setlocal

set "ROOT=%~dp0"
set "NUGET_CONFIG_FILE=%ROOT%NuGet.Config"
set "SOLUTION=%ROOT%CarSalesManagementSystem.sln"
set "PROJECT=%ROOT%DataAccessObjects\DataAccessObjects.csproj"
set "STARTUP_PROJECT=%ROOT%CarSalesManagementSystemAPI\CarSalesManagementSystemAPI.csproj"

if "%~1"=="" (
    echo Usage: ef.cmd ^<dotnet-ef-args^>
    echo Example: ef.cmd migrations list
    echo Example: ef.cmd database update
    exit /b 1
)

echo Building backend solution with repo NuGet.Config...
dotnet build "%SOLUTION%"
if errorlevel 1 exit /b %errorlevel%

echo Running dotnet ef %* ...
dotnet ef %* --no-build --project "%PROJECT%" --startup-project "%STARTUP_PROJECT%"
exit /b %errorlevel%
