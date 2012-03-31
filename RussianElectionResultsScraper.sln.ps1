function global:RunApplicationProject( [Parameter(Mandatory=$true)][string] $projectName,
                                [Parameter(Mandatory=$true)][string]        $commandLine,
                                [bool]                                      $underDebugger
                                )
    {
    $projects = $dte.ActiveSolutionProjects
    $startupProjectProperty = $dte.Solution.Properties.Item("StartupProject")
    Write-Host( "Current startup project: " + $startupProjectProperty.Value )
    
    #$project 
    
    $project = Get-Project $projectName 
    if ( $project -ne $null )
        {
        $startupProjectProperty.Value = $projectName
        $project.ConfigurationManager.ActiveConfiguration.Properties.Item( "StartArguments" ).Value = $commandLine
        #DTE.ExecuteCommand("Debug.StartWithoutDebugging")
        $dte.ExecuteCommand("Debug.Start")
        }
    else
        {
        Write-Error "Project '$projectName' is not found in solution"
        }
    }
    
function global:rcdpres2008()
    {
    $private:projectName = "RussianElectionResultsScraper.Console"
    $private:connectionString = '"Data Source=localhost;Initial Catalog=erp;Trusted_Connection=true"'
    $private:configDir = Split-Path -Parent ( ( Get-Project $projectName ).FullName )
    
    $private:config = '"' + ( Join-Path $configDir "pres2008.config" )  + '"'
    
    $private:url = '"http://www.vybory.izbirkom.ru/region/region/izbirkom?action=show&root=1&tvd=100100022249920&vrn=100100022176412&region=0&global=1&sub_region=0&prver=0&pronetvd=null&vibid=100100022249920&type=226"'
    
    RunApplicationProject `
        -ProjectName $projectName `
        -CommandLine "scrape --connectionString=$connectionString --config=$config --url=$url --recursive --cache"
    }


    