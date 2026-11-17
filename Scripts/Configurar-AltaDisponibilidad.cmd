@echo off
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File "%~dp0Configurar-AltaDisponibilidad.ps1"
exit /b %errorlevel%
