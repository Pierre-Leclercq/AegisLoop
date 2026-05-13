@echo off
echo ==========================================
echo AegisLoop V1 — Build Script (.NET 10)
echo ==========================================
echo.

echo [1/3] Restoration des packages .NET...
dotnet restore AegisLoop.sln
if %errorlevel% neq 0 (
    echo ERREUR: dotnet restore a echoue
    exit /b 1
)

echo.
echo [2/3] Build de la solution .NET...
dotnet build AegisLoop.sln --no-restore --configuration Release
if %errorlevel% neq 0 (
    echo ERREUR: dotnet build a echoue
    exit /b 1
)

echo.
echo [3/3] Build du frontend Electron...
cd src\desktop-electron
call npm install
call npm run build
cd ..\..

echo.
echo ==========================================
echo Build termine avec succes !
echo ==========================================