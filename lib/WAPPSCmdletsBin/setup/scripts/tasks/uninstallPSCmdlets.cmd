@ECHO off
%~d0
cd "%~dp0"

IF EXIST %WINDIR%\SysWow64 (
	SET powerShellDir=%WINDIR%\SysWow64\windowspowershell\v1.0
) ELSE (
	SET powerShellDir=%WINDIR%\system32\windowspowershell\v1.0
)

CALL %powerShellDir%\powershell.exe -NonInteractive -Command "Set-ExecutionPolicy unrestricted"

ECHO ===============================================
ECHO Uninstalling Windows Azure PowerShell Cmdlets
ECHO ===============================================

ECHO.
%powerShellDir%\powershell.exe -NonInteractive -command ". .\uninstallPSSnapIn.ps1"

ECHO.
%powerShellDir%\powershell.exe -NonInteractive -command ". .\uninstallPSModule.ps1"

ECHO.
PAUSE