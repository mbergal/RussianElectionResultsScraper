Function Get-OperatingSystemVersion
{
 (Get-WmiObject -Class Win32_OperatingSystem).Version
} #end Get-OperatingSystemVersion

Function Test-ModulePath
{


 [string] $documentsFolder = [System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::MyDocuments)
 if (Test-Path "$documentsFolder")
 { 
  $VistaPath = Join-Path $documentsFolder "WindowsPowerShell\Modules"
  $XPPath = Join-Path $documentsFolder "WindowsPowerShell\Modules"
 }
 else
 {
  $VistaPath = "$env:userProfile\documents\WindowsPowerShell\Modules"
  $XPPath =  "$env:Userprofile\my documents\WindowsPowerShell\Modules" 
 }

 if ([int](Get-OperatingSystemVersion).substring(0,1) -ge 6)
   { 
     if(-not(Test-Path -path $VistaPath))
       {
         New-Item -Path $VistaPath -itemtype directory | Out-Null
       } #end if
   } #end if
 Else 
   {  
     if(-not(Test-Path -path $XPPath))
       {
         New-Item -path $XPPath -itemtype directory | Out-Null
       } #end if
   } #end else
} #end Test-ModulePath

Function Copy-Module
{
    $UserPath = $env:PSModulePath.split(";")[0]
    $ModulePath = Join-Path $userPath "WAPPSCmdlets"

    if(-not(Test-Path -path $ModulePath))
      {
        New-Item -path $ModulePath -itemtype directory | Out-Null
      }
    
 	Copy-item -path "..\..\..\release\*" -destination $ModulePath
    Rename-Item "$ModulePath\Microsoft.WindowsAzure.Samples.ManagementTools.Powershell.dll" "WAPPSCmdlets.dll"
    Rename-Item "$ModulePath\Microsoft.WindowsAzure.Samples.ManagementTools.Powershell.dll-Help.xml" "WAPPSCmdlets.dll-Help.xml"
    echo "The module has been installed for the Current User to: $ModulePath"
}

Test-ModulePath
Copy-Module