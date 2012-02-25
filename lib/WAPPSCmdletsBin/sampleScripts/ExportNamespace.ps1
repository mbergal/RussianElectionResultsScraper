Param($sourceNamespace = "[your namespace]",
      $sourceManagementKey = "[your namespace management key]",
      [string]$fileToExport = "[path to output file]")  

function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value
    Split-Path $Invocation.MyCommand.Path
}

$scriptDirectory = Get-ScriptDirectory
Set-Location $scriptDirectory

.\AddSnapInAndModule.ps1

$sourceToken = Get-AcsManagementToken -Namespace $sourceNamespace -ManagementKey $sourceManagementKey
$acsNamespaceInfo = New-Object Microsoft.Samples.DPE.ACS.ServiceManagementTools.PowerShell.Model.ServiceNamespace

"Getting all the Identity Providers from $sourceNamespace namespace..."
$acsNamespaceInfo.IdentityProviders = Get-IdentityProvider -MgmtToken $sourceToken

"Getting all the Relying Parties from $sourceNamespace namespace..."
$acsNamespaceInfo.RelyingParties = @()
foreach ($s in Get-RelyingParty -MgmtToken $sourceToken)
{
    $acsNamespaceInfo.RelyingParties += @(Get-RelyingParty -MgmtToken $sourceToken -Name $s.Name)
}

"Getting all the Rule Groups from $sourceNamespace namespace..."
$acsNamespaceInfo.RuleGroups = @()
foreach ($s in Get-RuleGroup -MgmtToken $sourceToken)
{
    $acsNamespaceInfo.RuleGroups += @(Get-RuleGroup -MgmtToken $sourceToken -Name $s.Name)
}

"Getting all the Service Keys from $sourceNamespace namespace..."
$acsNamespaceInfo.ServiceKeys = Get-ServiceKey -MgmtToken $sourceToken

"Getting all the Service Identities from $sourceNamespace namespace..."
$acsNamespaceInfo.ServiceIdentities = Get-ServiceIdentity -MgmtToken $sourceToken

"Serializing all the information in $sourceNamespace namespace to the $fileToExport file..."
if (! [System.IO.Path]::IsPathRooted("$fileToExport"))
{
	$fileToExport = Join-Path "$scriptDirectory" "$fileToExport"
}
$acsNamespaceInfo.Serialize($fileToExport)

""
"Done"