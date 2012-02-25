@ECHO off
%~d0
cd "%~dp0"


SET assemblyPath="..\..\..\release\Microsoft.WindowsAzure.Samples.ManagementTools.Powershell.dll"

ECHO.
ECHO Installing PSSnapIn...
ECHO.

IF EXIST %WINDIR%\SysWow64 (
	%WINDIR%\Microsoft.NET\Framework64\v2.0.50727\InstallUtil.exe -i %assemblyPath%
	ECHO.
) 

%WINDIR%\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe -i %assemblyPath%

ECHO.