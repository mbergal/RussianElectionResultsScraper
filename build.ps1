Framework( "4.0" )

$scriptDirectory = Split-Path -parent $MyInvocation.MyCommand.ScriptBlock.File
$utilsPath = (Resolve-Path (Join-Path $scriptDirectory "src\build\utils.psm1"))

Import-Module $utilsPath -Force -ErrorAction Stop -Global

$psake.verboseError = $true

Load-ConfigFiles $scriptDirectory 'azure'

Properties {
    $solutionFileName = 'RussianElectionResultsScraper.sln'
    $localDatabaseName = Get-LocalDatabaseName
    $packageOutputDirectory = "$scriptDirectory\src\RussianElectionResultsScraper.Azure\bin\Debug\app.publish"
    $cloudServiceConfigurationFile = "$packageOutputDirectory\ServiceConfiguration.Cloud.cscfg" 
    $cloudServicePackage = "$packageOutputDirectory\RussianElectionResultsScraper.Azure.cspkg"
    }

Task Default -depends Build

Task Build {
    BuildSolution -Solution $solutionFileName  -Target Build -Configuration Debug
    }

Task ExportDb {
    LoadNecessaryAssemblies
    $localSqlServerConnection = Get-LocalSqlServerConnection
    $localSqlServerConnection.Connect()
    
	$dacpac = "$scriptDirectory\src\RussianElectionResultsScraper\data\$localDatabaseName.dacpac"
	$dacStore = New-Object -TypeName Microsoft.SqlServer.Management.Dac.DacStore $localSqlServerConnection
	$dacStore.Export( $localDatabaseName, $dacpac )
	}
	
Task UploadDb {
    LoadNecessaryAssemblies
    $credentials = New-Object -TypeName Microsoft.WindowsAzure.StorageCredentialsAccountAndKey( $Env:AZURESECURITYID, $Env:AZURESTORAGEACCESSKEY )
    $account = New-Object -TypeName Microsoft.WindowsAzure.CloudStorageAccount( $credentials, $true )
    $database = Get-LocalDatabaseName
    
    $blobClient = [Microsoft.WindowsAzure.StorageClient.CloudStorageAccountStorageClientExtensions]::CreateCloudBlobClient( $account )
    $blobClient.Timeout = New-TimeSpan -Minutes 10
    $blobContainer = $blobClient.GetContainerReference("backups")
    $cloudBlob = $blobContainer.GetBlobReference("$database.dacpac");
    
    using_ ( $fileStream = [System.IO.File]::OpenRead( ( join-path ( join-path ( Split-Path -parent $MyInvocation.MyCommand.ScriptBlock.File ) "data" ) ( $database + ".dacpac" ) ) )) {
        $cloudBlob.UploadFromStream($fileStream);
        }     
    }
      
Task RestoreDbOnAzure {
    LoadNecessaryAssemblies
    DacIESvcCli.exe -Import -Database erp -Server kswo10weza.database.windows.net -U "$Env:AZURESECURITYID" -P "$Env:AZUREDATABASEPASSWORD" -EDITION web -SIZE 1 -BLOBURL http://mbergal.blob.core.windows.net/backups/erp.dacpac -BLOBACCESSKEY "$Env:AZURESTORAGEACCESSKEY" -ACCESSKEYTYPE storage|more
    }
    
   
Task Package {
    PackageApplicationForCloud `
        -Solution $solutionFileName `
        -Target RussianElectionResultsScraper_Azure:Publish `
        -Configuration Debug `
        -ServiceConfigurationFile $cloudServiceConfigurationFile `
        -CloudSqlServer $Env:AZURESQLSERVER `
        -CloudSqlServerUserID $Env:AZURESECURITYID `
        -CloudSqlServerDatabase "erp" `
        -CloudSqlServerPassword $Env:AZURESQLSERVERPASSWORD 
       }
    
    
Task Deploy {
    DeployToCloud `
        -CertificateThumbprint $Env:AZURECertificateThumbprint `
        -SubscriptionId $Env:AZURESUBSCRIPTIONID `
        -Service ruelectstats `
        -Slot Production `
        -Package 'C:\Users\Misha\Stuff\RussianElectionResultsScraper\src\RussianElectionResultsScraper.Azure\bin\Debug\app.publish\RussianElectionResultsScraper.Azure.cspkg'  `
        -Configuration $cloudServiceConfigurationFile `
        -StorageServiceUserId $Env:AZURESECURITYID `
    }
    

function Get-LocalSqlServerConnection()
    {
	$configFileMap = New-Object -TypeName "System.Configuration.ExeConfigurationFileMap" 
	$configFileMap.ExeConfigFilename = "C:\Users\Misha\Stuff\RussianElectionResultsScraper\src\RussianElectionResultsScraper\app.config"
	$config = [System.Configuration.ConfigurationManager]::OpenMappedExeConfiguration( $configFileMap, [System.Configuration.ConfigurationUserLevel]::None );
    
	$serverConnection = New-Object -TypeName Microsoft.SqlServer.Management.Common.ServerConnection
	$serverConnection.ConnectionString = $config.ConnectionStrings.ConnectionStrings[ 'Elections' ].ConnectionString

    return $serverConnection
    }

function Get-LocalDatabaseName()
    {
    LoadNecessaryAssemblies
    $serverConnection = Get-LocalSqlServerConnection
	using_( $serverConnection ) {
        $serverConnection.Connect()
	    return $serverConnection.ExecuteScalar( "select db_name()" )
        }
    }

function LoadNecessaryAssemblies()
    {
    [Reflection.Assembly]::LoadWithPartialName( "Microsoft.SqlServer.Management.Dac, Version=11.0.0.0" )
    [Reflection.Assembly]::LoadWithPartialName( "Microsoft.SqlServer.Management.Dac, Version=11.0.0.0" ) 
    [Reflection.Assembly]::LoadWithPartialName( "Microsoft.SqlServer.ConnectionInfo, Version=11.0.0.0" )  | Out-Null
    [Reflection.Assembly]::LoadWithPartialName( "System.Configuration, Version=4.0.0.0" ) | Out-Null
    [Reflection.Assembly]::LoadFile( "$scriptDirectory\lib\Windows Azure SDK 1.6\Microsoft.WindowsAzure.Diagnostics.dll"  )
    [Reflection.Assembly]::LoadFile( "$scriptDirectory\lib\Windows Azure SDK 1.6\Microsoft.WindowsAzure.StorageClient.dll" ) | Out-Null
    [Reflection.Assembly]::LoadWithPartialName( "Microsoft.SqlServer.Management.Dac, Version=11.0.0.0" ) | Out-Null
    [Reflection.Assembly]::LoadWithPartialName( "Microsoft.SqlServer.ConnectionInfo, Version=11.0.0.0" ) | Out-Null
    [Reflection.Assembly]::LoadWithPartialName( "System.Configuration, Version=4.0.0.0" ) | Out-Null
    [Reflection.Assembly]::LoadWithPartialName( "Microsoft.WindowsAzure.StorageClient, Version=1.1.0.0" ) | Out-Null
    Import-Module "$scriptDirectory\lib\WAPPSCmdletsBin\release\Microsoft.WindowsAzure.Samples.ManagementTools.PowerShell.dll" -ErrorAction Stop     
    }

function BuildSolution( [string]$solution, 
                        [string]$target, 
                        [string]$configuration )
    {
    LoadNecessaryAssemblies
    Exec { msbuild $solutionFileName /t:$target /p:Configuration=$configuration }
    }

function PackageApplicationForCloud ( [Parameter(Mandatory=$true)][string]$solution,
                                      [Parameter(Mandatory=$true)][string]$target,
                                      [Parameter(Mandatory=$true)][string]$configuration,
                                      [Parameter(Mandatory=$true)][string]$serviceConfigurationFile,
                                      [Parameter(Mandatory=$true)][string]$cloudSqlServer,
                                      [Parameter(Mandatory=$true)][string]$cloudSqlServerDatabase,
                                      [Parameter(Mandatory=$true)][string]$cloudSqlServerUserID,
                                      [Parameter(Mandatory=$true)][string]$cloudSqlServerPassword ) 
    {
    LoadNecessaryAssemblies
    Exec { msbuild $solution /t:RussianElectionResultsScraper_Azure:Publish /p:Configuration=Debug }
    $xml = New-Object XML
    $xml.Load( $serviceConfigurationFile )
    $connectionStringSetting = Select-Xml -Xml $xml -XPath '//d:Role/d:ConfigurationSettings/d:Setting[@name="ConnectionString"]' -Namespace @{ d="http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration" }
    $connectionStringSetting.Node.value = "Server=tcp:$cloudSqlServer,1433;Database=$cloudSqlServerDatabase;User ID=$cloudSqlServerUserID;Password=$Env:AZURESQLSERVERPASSWORD;Trusted_Connection=False;Encrypt=True;"
    $xml.Save( $serviceConfigurationFile )
    }
    
function DeployToCloud( [Parameter(Mandatory=$true)][string]$certificateThumbprint,
                        [Parameter(Mandatory=$true)][string]$subscriptionId,
                        [Parameter(Mandatory=$true)][string]$service,
                        [Parameter(Mandatory=$true)][string]$slot,
                        [Parameter(Mandatory=$true)][string]$package,
                        [Parameter(Mandatory=$true)][string]$configuration,
                        [Parameter(Mandatory=$true)][string]$storageServiceUserId
                        )
    {
    LoadNecessaryAssemblies

    $certPath = "cert:\CurrentUser\MY\$certificateThumbprint"
    $cert = get-item $certPath

    Write-Output 'Read certificate' $cert
    function MakeHostedServiceFactory( [string]$hostedService, 
                                       [System.Security.Cryptography.X509Certificates.X509Certificate] $certificate,
                                       [string]$subscriptionId,
                                       [string]$slot,
                                       [string]$scriptDirectory) {
        return {
            Import-Module "$scriptDirectory\lib\WAPPSCmdletsBin\release\Microsoft.WindowsAzure.Samples.ManagementTools.PowerShell.dll" -ErrorAction Stop     
            
            $service = Get-HostedService -ServiceName $hostedService -Certificate $certificate -SubscriptionId $subscriptionId
            try {
                 $deployment = $service | Get-Deployment -Slot $slot
                }
            catch {}
             if ( $deployment -ne $null ) { return $deployment }
             else { return $service }
             }.GetNewClosure()
        }

    $getHostedService = MakeHostedServiceFactory -HostedService $service -certificate $cert -subscriptionId $subscriptionId -slot $slot -scriptDirectory $scriptDirectory

    $hostedService = &$getHostedService
     
    if ($hostedService.Status -ne $null) {
        Write-Output 'Found service:' $hostedService 
        SuspendDeployment( $hostedService )
        DeleteDeployment( $hostedService )
        }
    else
        {
        Write-Output "Service '$service' does not exist"
        }
    Write-Output 'Creating new service...'
    &$getHostedService |
        New-Deployment `
            -Slot $slot `
            -Package $package `
            -Configuration $configuration `
            -Label 'AAAAAA'  `
            -ServiceName $service `
            -StorageServiceName $storageServiceUserId |
        Get-OperationStatus -WaitToComplete
    Write-Output '...done'        
    
    StartDeployment( $hostedService )
    $deployment = &$getHostedService
    $deployment
    }

function SuspendDeployment( [Parameter(Mandatory=$true)]$deployment )
    {
    Write-Output '   Suspending deployment...'
    $hostedService |
      Set-DeploymentStatus 'Suspended' |
      Get-OperationStatus -WaitToComplete
    Write-Output '    ...done.'
    }
    
function DeleteDeployment( [Parameter(Mandatory=$true)]$deployment )
    {
    Write-Output '    Removing deployment...'
        $hostedService |
          Remove-Deployment |
          Get-OperationStatus -WaitToComplete
    Write-Output '    ...done.'          
    }
    
function StartDeployment( [Parameter(Mandatory=$true)]$deployment )
    {
    Write-Output '    Starting deployment...'
    &$getHostedService |
        Set-DeploymentStatus 'Running' | 
        Get-OperationStatus -WaitToComplete
    Write-Output '    ...done.'          
    }
