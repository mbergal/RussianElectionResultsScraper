$fsh = New-Module -Name "FileSystemHelper" -AsCustomObject {
    function Exists( $file )
        {
        return [System.IO.File]::Exists( $file )
        }
    Export-ModuleMember -Variable * -Function *                
    }

function Get-ConfigFiles( [string]$startDirectory, [string]$prefix, [string]$computerName, $FileSystemHelper )
    {
    if ( $FileSystemHelper -eq $null ) {
        $FileSystemHelper = $fsh
        }
        
    if ( $computerName -ne $null ) {
        $computerName = $Env:COMPUTERNAME
        }
        
    $private:dirs = ( Get-ParentDirs $startDirectory ) +  $startDirectory
    $private:configFiles = @()
    foreach( $d in $dirs ) {
        $private:config = $null;
        $private:localMachineConfig = $null;
        $private:tryPaths = @( "$prefix.config.ps1", "$prefix.$computerName.config.ps1" )
        
        foreach( $private:tryPath in $tryPaths ) {
            $tryPath = Join-Path $d $tryPath
            if ( $filesystemhelper.Exists( $tryPath ) ) {
                $configFiles += $tryPath
                }
            }
        }
    return $configFiles
    }

function Load-ConfigFiles( [string]$startDirectory, [string]$prefix, [string]$computerName ) {
    $private:configFiles = Get-ConfigFiles $startDirectory $prefix $computerName 
    if ( $configFiles -ne $null ) {
        foreach( $private:configFile in $configFiles )
            {
            . $configFile
            }
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
    
function using_
    {
    param($obj, [scriptblock]$sb)

    try {
        & $sb
    } finally {
        if ($obj -is [IDisposable]) {
            $obj.Dispose()
            #(get-interface $obj ([IDisposable])).Dispose()
        }
    }
    }