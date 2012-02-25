Function Check-PSModuleInstallation
{
    if ((Get-Module -ListAvailable -Name WAPPSCmdlets -ErrorAction SilentlyContinue) -ne $null)
	{
		.\uninstallPSModule.cmd
	}
	else
	{
		Write-Host WAPPSCmdlets Module is not installed.
	}
}

Check-PSModuleInstallation