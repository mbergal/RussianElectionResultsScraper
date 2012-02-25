@ECHO off
%~d0
cd "%~dp0"

IF EXIST %WINDIR%\SysWow64 (
	SET powerShellDir=%WINDIR%\SysWow64\windowspowershell\v1.0
) ELSE (
	SET powerShellDir=%WINDIR%\system32\windowspowershell\v1.0
)

CALL %powerShellDir%\powershell.exe -NonInteractive -Command "Set-ExecutionPolicy unrestricted"

ECHO =============================================
ECHO Installing Windows Azure PowerShell Cmdlets
ECHO =============================================

CALL %powerShellDir%\powershell.exe -Command ".\copyDiagnosticsAssembly.ps1"

:START
ECHO.
ECHO Please select how you wish to install the Windows Azure PowerShell Cmdlets:
ECHO 1. Install as a PowerShell Snap-in
ECHO 2. Install as a PowerShell Module

SET /p InstallType=Please select an option: 

IF NOT "%InstallType%"=="1" IF NOT "%InstallType%"=="2" GOTO START

IF "%InstallType%"=="1" GOTO SNAPINS

IF "%InstallType%"=="2" GOTO MODULES

GOTO DONE

:SNAPINS
%powerShellDir%\powershell.exe -NonInteractive -command ". .\uninstallPSSnapIn.ps1"
CALL installPSSnapIn.cmd 
GOTO DONE

:MODULES
ECHO.
%powerShellDir%\powershell.exe -NonInteractive -command ". .\uninstallPSModuleUser.ps1"
%powerShellDir%\powershell.exe -NonInteractive -command ". .\installPSModuleUser.ps1"
GOTO DONE

:DONE
ECHO.
PAUSE