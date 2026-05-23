@echo off
set MODE=dev
if "%MODE%"=="dev" (
    rmdir /s /q bin obj >nul 2>&1
    dotnet build >nul
    dotnet run --no-build >nul
) else if "%MODE%"=="release" (
    if exist bin\ (
        dotnet run --no-build >nul
    ) else (
        dotnet build >nul
        dotnet run --no-build >nul
    )
) else (
    echo Error: Invalid MODE value. Use "dev" or "release".
)
