# Check to see if the Snap-In is registered in the computer
$count = 0;
foreach ($s in (Get-PSSnapIn -Registered | where { $_.Name -eq "WAPPSCmdlets"; })) { $count++; }
if ($count -ne 0)
{
    # If it is installed and not in the session, add it
    $count = 0;
    foreach ($s in (Get-PSSnapIn | where { $_.Name -eq "WAPPSCmdlets"; })) { $count++; }
    if ($count -eq 0)
    {
        "Adding WAPPSCmdlets Snap-in..."
        Add-PSSnapIn "WAPPSCmdlets"
    }
}

# Check to see if the Module is installed in the computer
$count = 0;
foreach ($s in (Get-Module -ListAvailable | where { $_.Name -eq "WAPPSCmdlets; })) { $count++; }
if ($count -ne 0)
{
    # If it is installed but not imported, do so
    $count = 0;
    foreach ($s in (Get-Module | where { $_.Name -eq "WAPPSCmdlets"; })) { $count++; }
    if ($count -eq 0)
    {
        "Importing WAPPSCmdlets Module..."
        Import-Module "WAPPSCmdlets"
    }
}