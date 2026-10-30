<#
.SYNOPSIS
    Starts both the .NET 9 Backend API and Angular Frontend for local development.

.DESCRIPTION
    Launches:
    - Backend API at http://localhost:5100 (API Docs: http://localhost:5100/swagger or /scalar/v1)
    - Angular Frontend at http://localhost:4200
#>

$backendPath = Join-Path $PSScriptRoot "backend\src\EnterpriseUserManagement.Api"
$frontendPath = Join-Path $PSScriptRoot "frontend\enterprise-user-management-ui"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Enterprise User Management Platform (EUMP) - Dev Launcher" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "Starting Backend (.NET 9 Web API) on http://localhost:5100..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "& { Write-Host '--- EUMP Backend API (Port 5100) ---' -ForegroundColor Green; cd '$backendPath'; dotnet run --launch-profile http }"

Write-Host "Starting Frontend (Angular 19) on http://localhost:4200..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "& { Write-Host '--- EUMP Frontend UI (Port 4200) ---' -ForegroundColor Yellow; cd '$frontendPath'; npm start }"

Write-Host ""
Write-Host "Both processes launched in dedicated terminal windows." -ForegroundColor Cyan
Write-Host "Interactive API Docs: http://localhost:5100/swagger (or /scalar/v1)" -ForegroundColor White
Write-Host "Frontend UI:          http://localhost:4200" -ForegroundColor White
Write-Host "==========================================================" -ForegroundColor Cyan
