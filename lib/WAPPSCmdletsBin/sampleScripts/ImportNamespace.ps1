Param($targetNamespace = "[your namespace]",
      $targetManagementKey = "[your namespace management key]",
      [string]$fileToImport = "[path to input file]")  

function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value
    Split-Path $Invocation.MyCommand.Path
}

$scriptDirectory = Get-ScriptDirectory
Set-Location $scriptDirectory

.\AddSnapInAndModule.ps1

$targetToken = Get-AcsManagementToken -Namespace $targetNamespace -ManagementKey $targetManagementKey

"Deserializing all the namespace information from the $fileToImport file..."
$acsNamespaceInfo = New-Object Microsoft.Samples.DPE.ACS.ServiceManagementTools.PowerShell.Model.ServiceNamespace
if (! [System.IO.Path]::IsPathRooted("$fileToImport"))
{
	$fileToImport = Join-Path "$scriptDirectory" "$fileToImport"
}
$acsNamespaceInfo.Deserialize($fileToImport)

# Identity Providers
if ($acsNamespaceInfo.IdentityProviders -ne $null)
{
    $count = $acsNamespaceInfo.IdentityProviders.Length
    "Importing $count Identity Providers to $targetNamespace namespace..."
    # If an Identity Provider with the same name exists, it is replaced (removed & added) by the new Identity Provider.
    foreach ($idp in $acsNamespaceInfo.IdentityProviders)
    {
        if (!$idp.SystemReserved)
        {
            $targetIdp = Get-IdentityProvider -Name $idp.Name -MgmtToken $targetToken
            if ($targetIdp -ne $null)
            {
                Remove-IdentityProvider -Name $idp.Name -MgmtToken $targetToken
            }
            Add-IdentityProvider -IdentityProvider $idp -MgmtToken $targetToken
        }
    }
}
else
{
    "There is not any Identity Provider to import."
}

# Rule Groups
if ($acsNamespaceInfo.RuleGroups -ne $null)
{
    $count = $acsNamespaceInfo.RuleGroups.Length
    "Importing $count Rule Groups and their Rules to $targetNamespace namespace..."
    # If a Rule Group with the same name exists, it is replaced (removed & added) by the new Rule Group.
    foreach ($sourceGroup in $acsNamespaceInfo.RuleGroups)
    {
        if (!$sourceGroup.SystemReserved)
        {
            $targetGroup = Get-RuleGroup -Name $sourceGroup.Name -MgmtToken $targetToken
            if ($targetGroup -ne $null)
            {
                Remove-RuleGroup -Name $sourceGroup.Name -MgmtToken $targetToken
            }
            Add-RuleGroup -RuleGroup $sourceGroup -MgmtToken $targetToken
        }
    }
}
else
{
    "There is not any Rule Group to import."
}

# Relying Parties
if ($acsNamespaceInfo.RelyingParties -ne $null)
{
    $count = $acsNamespaceInfo.RelyingParties.Length
    "Importing $count Relying Parties to $targetNamespace namespace..."
    # If a Relying Party with the same name exists, it is replaced (removed & added) by the new Relying Party.
    foreach ($fullRP in $acsNamespaceInfo.RelyingParties)
    {
        if (!$fullRP.SystemReserved)
        {
            $targetRP = Get-RelyingParty -Name $fullRP.Name -MgmtToken $targetToken
            if ($targetRP -ne $null)
            {
                Remove-RelyingParty -Name $fullRP.Name -MgmtToken $targetToken
            }
            Add-RelyingParty -RelyingParty $fullRP -MgmtToken $targetToken
        }
    }
}
else
{
    "There is not any Relying Party to import."
}

# Service Keys
if ($acsNamespaceInfo.ServiceKeys -ne $null)
{
    $count = $acsNamespaceInfo.ServiceKeys.Length
    "Importing $count Service Keys to $targetNamespace namespace..."
    # If a Service Key with the same name exists, it is replaced (removed & added) by the new Service Key.
    foreach ($key in $acsNamespaceInfo.ServiceKeys)
    {
        if (!$key.SystemReserved -and ($key.Usage -ne "Management"))
        {            
            if ($key.Name -ne $null)
            {
                $targetKey = Get-ServiceKey -Name $key.Name -MgmtToken $targetToken
                if ($targetKey -ne $null)
                {
                    Remove-ServiceKey -Name $key.Name -MgmtToken $targetToken
                }
            }

            if ($key.Usage -eq "Encrypting")
            {
                Add-TokenEncryptionKey -TokenEncryptionKey $key -MgmtToken $targetToken
            }
            elseif($key.Usage -eq "Signing")
            {
                Add-TokenSigningKey -TokenSigningKey $key -MgmtToken $targetToken
            }
        }
    }
}
else
{
    "There is not any Service Key to import."
}

# Service Identities
if ($acsNamespaceInfo.ServiceIdentities -ne $null)
{
    $count = $acsNamespaceInfo.ServiceIdentities.Length
    "Importing $count Service Identities to $targetNamespace namespace..."
    # If a Service Identity with the same name exists, it is replaced (removed & added) by the new Service Identity.
    foreach ($si in $acsNamespaceInfo.ServiceIdentities)
    {
        if (!$si.SystemReserved)
        {
            $targetSi = Get-ServiceIdentity -Name $si.Name -MgmtToken $targetToken
            if ($targetSi -ne $null)
            {
                Remove-ServiceIdentity -Name $si.Name -MgmtToken $targetToken
            }
            Add-ServiceIdentity -ServiceIdentity $si -MgmtToken $targetToken
        }
    }
}
else
{
    "There is not any Service Identity to import."
}

""
"Done"