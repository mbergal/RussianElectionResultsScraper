Param($sourceNamespace = "[your source namespace]",
      $sourceManagementKey = "[your source namespace management key]",
      $targetNamespace = "[your target namespace]",
      $targetManagementKey = "[your target namespace management key]")

function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value
    Split-Path $Invocation.MyCommand.Path
}

$scriptDirectory = Get-ScriptDirectory
Set-Location $scriptDirectory

.\AddSnapInAndModule.ps1

$sourceToken = Get-AcsManagementToken -Namespace $sourceNamespace -ManagementKey $sourceManagementKey
$targetToken = Get-AcsManagementToken -Namespace $targetNamespace -ManagementKey $targetManagementKey

# Identity Providers
$identityProviders = Get-IdentityProvider -MgmtToken $sourceToken
if ($identityProviders -ne $null)
{
    $count = $identityProviders.Length
    "Cloning $count Identity Providers from $sourceNamespace to $targetNamespace namespace..."
    # If an Identity Provider with the same name exists, it is replaced (removed & added) by the new Identity Provider.
    foreach ($idp in $identityProviders)
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
    "There is not any Identity Provider to clone."
}

# Rule Groups
$ruleGroups = Get-RuleGroup -MgmtToken $sourceToken
if ($ruleGroups -ne $null)
{
    $count = $ruleGroups.Length
    "Cloning $count Rule Groups and their Rules from $sourceNamespace to $targetNamespace namespace..."
    # If a Rule Group with the same name exists, it is replaced (removed & added) by the new Rule Group.
    foreach ($group in $ruleGroups)
    {
        $sourceGroup = Get-RuleGroup -Name $group.Name -MgmtToken $sourceToken
        if (!$group.SystemReserved)
        {
            $targetGroup = Get-RuleGroup -Name $group.Name -MgmtToken $targetToken
            if ($targetGroup -ne $null)
            {
                Remove-RuleGroup -Name $group.Name -MgmtToken $targetToken
            }
            Add-RuleGroup -RuleGroup $sourceGroup -MgmtToken $targetToken
        }
    }
}
else
{
    "There is not any Rule Group to clone."
}

# Relying Parties
$relyingParties = Get-RelyingParty -MgmtToken $sourceToken
if ($relyingParties -ne $null)
{
    $count = $relyingParties.Length
    "Cloning $count Relying Parties from $sourceNamespace to $targetNamespace namespace..."
    # If a Relying Party with the same name exists, it is replaced (removed & added) by the new Relying Party.
    foreach ($party in $relyingParties)
    {
        $fullRP = Get-RelyingParty -Name $party.Name -MgmtToken $sourceToken
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
    "There is not any Relying Party to clone."
}

# Service Keys
$keys = Get-ServiceKey -MgmtToken $sourceToken
if ($keys -ne $null)
{
    $count = $keys.Length
    "Cloning $count Service Keys from $sourceNamespace to $targetNamespace namespace..."
    # If a Service Key with the same name exists, it is replaced (removed & added) by the new Service Key.
    foreach ($key in $keys)
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
    "There is not any Service Key to clone."
}

# Service Identities
$serviceIdentities = Get-ServiceIdentity -MgmtToken $sourceToken
if ($serviceIdentities -ne $null)
{
    $count = $serviceIdentities.Length
    "Cloning $count Service Identities from $sourceNamespace to $targetNamespace namespace..."
    # If a Service Identity with the same name exists, it is replaced (removed & added) by the new Service Identity.
    foreach ($si in $serviceIdentities)
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
    "There is not any Service Identity to clone."
}

""
"Done"