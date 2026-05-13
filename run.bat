@echo off
REM AegisLoop V1 — Lanceur racine (.NET 10)
REM Usage: run          -> Desktop Electron (API+Worker+UI)
REM       run api       -> API seule (port 5100)
REM       run worker    -> Worker ingestion seul
REM       run desktop   -> Desktop Electron

if "%1"=="" goto desktop
if "%1"=="api" goto api
if "%1"=="worker" goto worker
if "%1"=="desktop" goto desktop
echo Usage: run [api^|worker^|desktop]
goto end

:api
echo Lancement de AegisLoop.Api (port 5100)...
dotnet run --project src\AegisLoop.Api\AegisLoop.Api.csproj
goto end

:worker
echo Lancement de AegisLoop.Worker (IngestionWorker)...
dotnet run --project src\AegisLoop.Worker\AegisLoop.Worker.csproj
goto end

:desktop
echo Lancement du Desktop Electron (API+Worker+UI)...
cd src\desktop-electron
call npm install
call npm run electron:dev
cd ..\..
goto end

:end