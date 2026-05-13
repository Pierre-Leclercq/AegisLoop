@echo off
echo ==========================================
echo AegisLoop V1 — Test Script (.NET 10)
echo ==========================================
echo.
echo Baseline officielle locale/CI: dotnet test AegisLoop.sln --configuration Release --settings .runsettings
echo Note: la configuration Debug peut etre bloquee localement par Windows Code Integrity / Smart App Control.
echo.

echo [1/2] Tests backend .NET...
dotnet test AegisLoop.sln --settings .runsettings --configuration Release --verbosity normal
if %errorlevel% neq 0 (
    echo ERREUR: Tests .NET echoues
    exit /b 1
)

echo.
echo [2/2] Tests frontend...
cd src\desktop-electron
call npm test
cd ..\..

echo.
echo ==========================================
echo Tests termines avec succes !
echo ==========================================