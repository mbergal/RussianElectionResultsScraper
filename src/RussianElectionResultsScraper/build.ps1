[Reflection.Assembly]::LoadWithPartialName( "Microsoft.SqlServer.Management.Dac, Version=11.0.0.0" ) | Out-Null
[Reflection.Assembly]::LoadWithPartialName( "Microsoft.SqlServer.ConnectionInfo, Version=11.0.0.0" ) | Out-Null
[Reflection.Assembly]::LoadWithPartialName( "System.Configuration, Version=4.0.0.0" ) | Out-Null

[Reflection.Assembly]::LoadFile( "C:\Users\Misha\Stuff\RussianElectionResultsScraper\lib\Windows Azure SDK 1.6\Microsoft.WindowsAzure.StorageClient.dll" ) | Out-Null

$scriptDirectory = Split-Path -parent $MyInvocation.MyCommand.ScriptBlock.File
$utilsPath = (Resolve-Path (Join-Path $scriptDirectory "..\build\utils.psm1"))

Import-Module $utilsPath -Force
$framework = '4.0'

[Reflection.Assembly]::LoadWithPartialName( "Microsoft.SqlServer.Management.Dac, Version=11.0.0.0" ) | Out-Null
[Reflection.Assembly]::LoadWithPartialName( "Microsoft.SqlServer.ConnectionInfo, Version=11.0.0.0" ) | Out-Null
[Reflection.Assembly]::LoadWithPartialName( "System.Configuration, Version=4.0.0.0" ) | Out-Null
[Reflection.Assembly]::LoadWithPartialName( "Microsoft.WindowsAzure.StorageClient, Version=1.1.0.0" ) | Out-Null

Task Default -depends Build

Task Build {
   Exec { msbuild "..\..\RussianElectionResultsScraper.sln" /t:Build /p:Configuration=Debug }
   }

Task ExportDb {

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
	}
	
Task UploadDb {
    $scriptDirectory
    Load-ConfigFiles  $scriptDirectory 'azure'
    $credentials = New-Object -TypeName Microsoft.WindowsAzure.StorageCredentialsAccountAndKey( $Env:AZURESECURITYID, $Env:AZURESTORAGEACCESSKEY )
    $account = New-Object -TypeName Microsoft.WindowsAzure.CloudStorageAccount( $credentials, $true )
    $blobClient = [Microsoft.WindowsAzure.StorageClient.CloudStorageAccountStorageClientExtensions]::CreateCloudBlobClient( $account )
    $blobContainer = $blobClient.GetContainerReference("backups")
    $cloudBlob = $blobContainer.GetBlobReference("a");
    using_ ( $fileStream = [System.IO.File]::OpenRead( 'C:\1.bmml')) {
        $cloudBlob.UploadFromStream($fileStream);
        }     
    }
      
Task RestoreDbOnAzure {
    # ToDo
    }