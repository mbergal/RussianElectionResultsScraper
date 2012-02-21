$scriptDirectory = Split-Path -parent $MyInvocation.MyCommand.ScriptBlock.File
$utilsPath = (Resolve-Path (Join-Path $scriptDirectory "..\build\utils.ps1"))

Import-Module $utilsPath
$framework = '4.0'

[Reflection.Assembly]::LoadWithPartialName( "Microsoft.SqlServer.Management.Dac, Version=11.0.0.0" ) | Out-Null
[Reflection.Assembly]::LoadWithPartialName( "System.Configuration, Version=4.0.0.0" )| Out-Null

Task Default -depends Build

Task Build {
   Exec { msbuild "..\..\RussianElectionResultsScraper.sln" /t:Build /p:Configuration=Debug }
   }

Task UploadDb {

	$configFileMap = New-Object -TypeName "System.Configuration.ExeConfigurationFileMap" 
	$configFileMap.ExeConfigFilename = "C:\Users\Misha\Stuff\RussianElectionResultsScraper\src\RussianElectionResultsScraper\app.config"
	$config = [System.Configuration.ConfigurationManager]::OpenMappedExeConfiguration( $configFileMap, [System.Configuration.ConfigurationUserLevel]::None );


	$serverConnection = New-Object -TypeName Microsoft.SqlServer.Management.Common.ServerConnection
	$serverConnection.ConnectionString = $config.ConnectionStrings.ConnectionStrings[ 'Elections' ].ConnectionString

	$serverConnection.Connect()
	$database = $serverConnection.ExecuteScalar( "select db_name()" )
	$dacpac = ( join-path ( join-path ( Split-Path -parent $MyInvocation.MyCommand.ScriptBlock.File ) "data" ) ( $database + ".dacpac" ) )
	$dacpac

	$dacStore = New-Object -TypeName Microsoft.SqlServer.Management.Dac.DacStore $serverConnection 
	$dacStore.Export( $database, $dacpac )
	
    

	# Upload-Blob
	# Restore-Db
	}
	