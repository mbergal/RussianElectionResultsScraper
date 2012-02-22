function Load-EnvironmentVars( $filePrefix, $location )
	{
	while ( 1 )
		{
		$location = Split-Path -Parent $location
		}
	}

function Get-ParentDirs( $dir )
    {
    $parents = @()
    
	$parentDir = $dir
    while ( $true ) {
        $parentDir = Split-Path -parent $parentDir
        if ( -not $parentDir ) { break; }
        $parents += $parentDir
        }
	[array]::Reverse($parents)
    return ,$parents;
    }