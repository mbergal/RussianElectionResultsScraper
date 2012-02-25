@ECHO off
%~d0
cd "%~dp0"

ECHO ====================================================
ECHO Uninstalling Windows Azure PowerShell Cmdlets Module
ECHO ====================================================

ECHO.
ECHO Uninstalling Module...
ECHO.

IF EXIST %WINDIR%\SysWow64 (
	SET powerShellDir=%WINDIR%\SysWow64\windowspowershell\v1.0
) ELSE (
	SET powerShellDir=%WINDIR%\system32\windowspowershell\v1.0
)

CALL %powerShellDir%\powershell.exe -NonInteractive -Command "Set-ExecutionPolicy unrestricted"

%powerShellDir%\powershell.exe -NonInteractive -command ". .\uninstallPSModuleUser.ps1"
