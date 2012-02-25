@ECHO off
%~d0
cd "%~dp0\scripts\tasks"

CScript uninstallPSCmdlets.vbs "%~dp0\scripts\tasks"