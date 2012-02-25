Function Remove-Module
{
    $UserPath = $env:PSModulePath.split(";")[0]
    $ModulePath = Join-Path $userPath "WAPPSCmdlets"
    if(Test-Path -path $ModulePath)
      {
        Remove-Item $ModulePath -recurse | Out-Null
      }
}

Function Check-PSModuleUninstallCorrectly
{
    if ((Get-Module -ListAvailable -Name WAPPSCmdlets -ErrorAction SilentlyContinue) -ne $null)
	{
		Write-Host The WAPPSCmdlets module could not be uninstalled. Please, make sure the module is not being used by another program, and try again.
	}
	else
	{
		Write-Host The uninstall has completed.
	}
}

Remove-Module
Check-PSModuleUninstallCorrectly