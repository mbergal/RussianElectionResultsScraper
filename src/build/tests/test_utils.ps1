
$here = Split-Path -Parent $MyInvocation.MyCommand.Path

[Reflection.Assembly]::LoadFile( 'C:\Users\Misha\Stuff\RussianElectionResultsScraper\packages\NHamcrest.1.2.1\lib\NHamcrest.dll' ) | Out-Null
[Reflection.Assembly]::LoadFile( 'C:\Users\Misha\Stuff\RussianElectionResultsScraper\packages\NUnit.2.5.10.11092\lib\nunit.framework.dll' ) | Out-Null

Import-Module "$here\..\..\..\lib\Pester\Pester.psm1" -Force
Import-Module "$here\..\utils.psm1"  -Force


Describe "Utils" {
    Describe "Get-ParentDirs" {
	    It "Should return empty list for disk root" {
			[NUnit.Framework.Assert]::AreEqual( ( Get-ParentDirs( 'C:\' ) ), @() )
			}
	    It "Should return list of parents for non-root directory" {
			[NUnit.Framework.Assert]::AreEqual( ( Get-ParentDirs( 'C:\aaaaa' ) ), @( 'C:\' ) )
			[NUnit.Framework.Assert]::AreEqual( ( Get-ParentDirs( 'C:\aaaaa\bbbb' ) ), @( 'C:\', 'C:\aaaaa' ) )
#			[NUnit.Framework.Assert]::AreEqual( ( Get-ParentDirs( 'C:\aaaaa\bbbb\cccc' ) ), @( 'C:\', 'C:\aaaaa', 'bbbb' ) )
			}
	}
}`
