Sample Scripts Usage
====================

> CloneNamespace.ps1 "[your source namespace]" "[your source namespace management key]" "[your target namespace]" "[your target namespace management key]" 

> ExportNamespace.ps1 "[your namespace]" "[your namespace management key]" "[path to output file]"

> ImportNamespace.ps1 "[your namespace]" "[your namespace management key]" "[path to input file]"

> PublishPackage.ps1 -cert $certificate -subsid "[subscription id]" -serviceName "[hosted service]" -slot "[hosted service slot]" -packageLocation "[location of the package]" -packageConfiguration "[configuration file of the package]" -deploymentLabel "[deployment label]" -deploymentName "[deployment name]" -storageServiceName "[storage service name]"
