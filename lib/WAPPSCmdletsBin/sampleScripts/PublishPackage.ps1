Param($cert,
      $subsid = "[subscription id]",
      $serviceName = "[hosted service]",
      $slot = "[hosted service slot]",
      $packageLocation = "[location of the package]",
	  $packageConfiguration = "[configuration file of the package]",
	  $deploymentLabel = "[deployment label]",
	  $deploymentName = "[deployment name]",
	  $storageServiceName = "[storage service name]",
	  $timeStampFormat = "o")

function SuspendDeployment()
{
	write-progress -id 1 -activity "Suspending Deployment" -status "In progress"
	Write-Output "$(Get-Date –f $timeStampFormat) - Suspending Deployment: In progress"

	$suspend = Set-DeploymentStatus -Slot $slot -ServiceName $serviceName -SubscriptionId $subsid -Certificate $cert -Status Suspended
	$opstat = Get-OperationStatus -SubscriptionId $subsid -Certificate $cert -operationid $suspend.operationId
	
	while ([string]::Equals($opstat, "InProgress"))
	{
		sleep -Seconds 1

		$opstat = Get-OperationStatus -SubscriptionId $subsid -Certificate $cert -operationid $suspend.operationId
	}

	write-progress -id 1 -activity "Suspending Deployment" -status $opstat
	Write-Output "$(Get-Date –f $timeStampFormat) - Suspending Deployment: $opstat"
}

function DeleteDeployment()
{
	SuspendDeployment

	write-progress -id 2 -activity "Deleting Deployment" -Status "In progress"
	Write-Output "$(Get-Date –f $timeStampFormat) - Deleting Deployment: In progress"

	$removeDeployment = Remove-Deployment -Slot $slot -ServiceName $serviceName -SubscriptionId $subsid -Certificate $cert
	$opstat = WaitToCompleteNoProgress($removeDeployment.operationId)
	
	write-progress -id 2 -activity "Deleting Deployment" -Status $opstat
	Write-Output "$(Get-Date –f $timeStampFormat) - Deleting Deployment: $opstat"
	
	sleep -Seconds 10
}

function WaitToCompleteNoProgress($operationId)
{
	$result = Get-OperationStatus -SubscriptionId $subsid -Certificate $cert -OperationId $operationId
	
	while ([string]::Equals($result, "InProgress"))
	{
		sleep -Seconds 1
		$result = Get-OperationStatus -SubscriptionId $subsid -Certificate $cert -OperationId $operationId
	}
	
	return $result
}

function CheckForExistingDeployment()
{
	$deployment = Get-Deployment -ServiceName $serviceName -Slot $slot -Certificate $cert -SubscriptionId $subsid

	if ($deployment.Name -ne $null)
	{
		$title = "Delete Deployment"
		$message = "The selected deployment environment is in use, would you like to delete and continue?`nHosted Service: $serviceName`nDeploymentEnvironment: $slot`nDeploymentLabel: $deploymentLabel"

		$yes = New-Object System.Management.Automation.Host.ChoiceDescription "&Yes", "Delete and Continue"

		$no = New-Object System.Management.Automation.Host.ChoiceDescription "&No", "Cancel"

		$options = [System.Management.Automation.Host.ChoiceDescription[]]($yes, $no)

		$result = $host.ui.PromptForChoice($title, $message, $options, 1) 

		switch ($result)
	    {
	        0 
			{
				DeleteDeployment
			}
	        1 
			{
				Write-Output "$(Get-Date –f $timeStampFormat) - Script execution cancelled."
				exit
			}
	    }
	}
}

function CreateNewDeployment()
{
	$deployment = Get-Deployment -ServiceName $serviceName -Slot $slot -Certificate $cert -SubscriptionId $subsid

	while ($deployment.Name -ne $null)
	{
		sleep -Seconds 1
		$deployment = Get-Deployment -ServiceName $serviceName -Slot $slot -Certificate $cert -SubscriptionId $subsid
	}

	write-progress -id 3 -activity "Creating New Deployment" -Status "In progress"
	Write-Output "$(Get-Date –f $timeStampFormat) - Creating New Deployment: In progress"

	$newdeployment = New-Deployment -Slot $slot -Package $packageLocation -Configuration $packageConfiguration -label $deploymentLabel -Name $deploymentName -ServiceName $serviceName -StorageServiceName $storageServiceName -SubscriptionId $subsid -Certificate $cert
	$opstat = WaitToCompleteNoProgress($newdeployment.operationId)
	
	write-progress -id 3 -activity "Creating New Deployment" -Status $opstat
	Write-Output "$(Get-Date –f $timeStampFormat) - Creating New Deployment: $opstat"

	StartInstances
}

function StartInstances()
{
	write-progress -id 4 -activity "Starting Instances" -status "In progress"
	Write-Output "$(Get-Date –f $timeStampFormat) - Starting Instances: In progress"

	$run = Set-DeploymentStatus -Slot $slot -ServiceName $serviceName -SubscriptionId $subsid -Certificate $cert -Status Running
	
	$deployment = Get-Deployment -ServiceName $serviceName -Slot $slot -Certificate $cert -SubscriptionId $subsid
	$oldStatusStr = @("") * $deployment.RoleInstanceList.Count
	
	while (-not(AllInstancesRunning($deployment.RoleInstanceList)))
	{
		$i = 1
		foreach ($roleInstance in $deployment.RoleInstanceList)
		{
			$instanceName = $roleInstance.InstanceName
			$instanceStatus = $roleInstance.InstanceStatus

			if ($oldStatusStr[$i - 1] -ne $roleInstance.InstanceStatus)
			{
				$oldStatusStr[$i - 1] = $roleInstance.InstanceStatus
				Write-Output "$(Get-Date –f $timeStampFormat) - Starting Instance '$instanceName': $instanceStatus"
			}

			write-progress -id (4 + $i) -activity "Starting Instance '$instanceName'" -status "$instanceStatus"
			$i = $i + 1
		}

		sleep -Seconds 1

		$deployment = Get-Deployment -ServiceName $serviceName -Slot $slot -Certificate $cert -SubscriptionId $subsid
	}

	$i = 1
	foreach ($roleInstance in $deployment.RoleInstanceList)
	{
		$instanceName = $roleInstance.InstanceName
		$instanceStatus = $roleInstance.InstanceStatus

		if ($oldStatusStr[$i - 1] -ne $roleInstance.InstanceStatus)
		{
			$oldStatusStr[$i - 1] = $roleInstance.InstanceStatus
			Write-Output "$(Get-Date –f $timeStampFormat) - Starting Instance '$instanceName': $instanceStatus"
		}

		write-progress -id (4 + $i) -activity "Starting Instance '$instanceName'" -status "$instanceStatus"
		$i = $i + 1
	}
	
	$opstat = Get-OperationStatus -SubscriptionId $subsid -Certificate $cert -operationid $run.operationId
	
	write-progress -id 4 -activity "Starting Instances" -status $opstat
	Write-Output "$(Get-Date –f $timeStampFormat) - Starting Instances: $opstat"
}

function AllInstancesRunning($roleInstanceList)
{
	foreach ($roleInstance in $roleInstanceList)
	{
		if ($roleInstance.InstanceStatus -ne "Ready")
		{
			return $false
		}
	}
	
	return $true
}

Write-Output "$(Get-Date –f $timeStampFormat) - Script execution started."

CheckForExistingDeployment
	
CreateNewDeployment

Write-Output "$(Get-Date –f $timeStampFormat) - Script execution finished."

