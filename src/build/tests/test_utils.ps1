$here = Split-Path -Parent $MyInvocation.MyCommand.Path

[Reflection.Assembly]::LoadFile( 'C:\Users\Misha\Stuff\RussianElectionResultsScraper\packages\NUnit.2.5.10.11092\lib\nunit.framework.dll' ) | Out-Null

Import-Module "$here\..\..\..\lib\Pester\Pester.psm1" -Force 
Import-Module "$here\..\utils.psm1" -Force

trap { write-error $_; get-stacktrace }
function get-stacktrace
{
    trap { continue }
    1..100 | %{
        $inv = &{ gv -sc $_ myinvocation } 2>$null
        if ($inv) { write-host -for blue $inv.value.positionmessage.replace("`n","") }
        }
    exit
} 

$fsh = New-Module -Name "FakeFileSystemHelper" -AsCustomObject {
    function Exists( $file )
        {
        switch -wildcard ( $file )
            {
            "d:\*" { return $true; }
            "c:\environment.config" { return $true }
            "c:\environment.comp.config" { return $true }
            "c:\a\environment.config" { return $false }
            "c:\a\environment.comp.config" { return $false }
            "c:\a\environment.comp.config" { return $false }
            "c:\a\bb\environment.config" { return $true }
            "c:\a\bb\environment.comp.config" { return $true }
            default { throw "$file ?" }
            }
        }
    Export-ModuleMember -Variable * -Function *                
    }
    
Describe "Utils" {
    Describe "Get-ParentDirs" {
	    It "Should return empty list for disk root" {
			[NUnit.Framework.Assert]::AreEqual( ( Get-ParentDirs( 'C:\' ) ), @() )
			}
	    It "Should return list of parents for non-root directory" {
			[NUnit.Framework.Assert]::AreEqual( ( Get-ParentDirs( 'C:\aaaaa' ) ), @( 'C:\' ) )
			[NUnit.Framework.Assert]::AreEqual( ( Get-ParentDirs( 'C:\aaaaa\bbbb' ) ), @( 'C:\', 'C:\aaaaa' ) )
			[NUnit.Framework.Assert]::AreEqual( ( Get-ParentDirs( 'C:\aaaaa\bbbb\cccc' ) ), @( 'C:\', 'C:\aaaaa', 'C:\aaaaa\bbbb' ) )
			}
        }          
    Describe "Get-ConfigFiles" {
        
        It "Should return" {
            $a = Get-ConfigFiles -Prefix 'environment' -StartDirectory 'c:\a\bb' -ComputerName 'comp' -FileSystemHelper $fsh
            [NUnit.Framework.Assert]::AreEqual( $a, @( 'c:\environment.config', 'c:\environment.comp.config', 'c:\a\bb\environment.config', 'c:\a\bb\environment.comp.config' ) )
            }
            
        It "should use the name of computer if -ComputerName is not specified" {
            $Env:COMPUTERNAME = "comp"
            $a = Get-ConfigFiles -Prefix 'e' -StartDirectory 'd:\' -FileSystemHelper $fsh
            [NUnit.Framework.Assert]::AreEqual( $a, @( 'd:\e.config', 'd:\e.comp.config' )  )
            }
	}
}`

