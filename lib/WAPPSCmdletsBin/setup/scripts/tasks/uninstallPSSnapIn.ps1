Function Check-PPSnapinInstallation
{
    if ((Get-PSSnapin -r -Name "WAPPSCmdlets" -ErrorAction SilentlyContinue) -ne $null)
	{
		.\uninstallPSSnapIn.cmd
	}
	else
	{
		Write-Host WAPPSCmdlets PSSnapIn is not installed.
	}
}

Check-PPSnapinInstallation