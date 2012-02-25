$Keys = Get-ChildItem "HKLM:\SOFTWARE\Microsoft\Microsoft SDKs\ServiceHosting\"
$Items = $keys | foreach-object {Get-ItemProperty $_.PsPath}
$NewestItem = $Items | select InstallPath -First 1 | sort -Property FullVersion -Descending

$path = Join-Path $NewestItem.InstallPath "\ref\Microsoft.WindowsAzure.Diagnostics.*"

Copy-item -path $path -destination "..\..\..\release"