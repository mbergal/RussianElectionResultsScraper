@echo off
%~d0
cd "%~dp0"

ECHO =======================================================
ECHO Uninstalling Windows Azure PowerShell Cmdlets Snap-in
ECHO =======================================================

SET assemblyPath="..\..\..\release\Microsoft.WindowsAzure.Samples.ManagementTools.Powershell.dll"

ECHO.
ECHO Uninstalling PSSnapIn...
ECHO.

IF EXIST %WINDIR%\SysWow64 (
	%WINDIR%\Microsoft.NET\Framework64\v2.0.50727\InstallUtil.exe -u %assemblyPath%
	ECHO.
) 

%WINDIR%\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe -u %assemblyPath%

ECHO.