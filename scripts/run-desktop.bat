@echo off
echo ==========================================
echo AegisLoop V1 — Desktop Electron (.NET 10)
echo Electron lance automatiquement API + Worker
echo ==========================================
echo.
cd src\desktop-electron
call npm install
call npm run electron:dev