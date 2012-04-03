$global:projectName = "RussianElectionResultsScraper.Console"
$global:webProjectName = "RussianElectionResultsScraper.Web"
$global:connectionString = '"Data Source=localhost;Initial Catalog=erp;Trusted_Connection=true"'
$global:connectionString2 = '"Data Source=localhost;Initial Catalog=erp2;Trusted_Connection=true"'
$global:url2 = '"http://localhost:62457/database"'

    
function global:Check( [string]$electionId )
    {
    RunApplicationProject `
        -ProjectName $projectName `
        -CommandLine "check --connectionString=$connectionString --election=$electionId"
    }
    
function global:Scrape( [string]$electionId )
    {
    $private:url = Get-ElectionRootUrl( $electionId )
    $private:config = Get-Config( $electionId )
    
    RunApplicationProject `
        -ProjectName $projectName `
        -CommandLine "scrape --connectionString=$connectionString --config=$config --url=$url --recursive --cache"
    }

function global:Upgrade()
    {
    RunApplicationProject `
        -ProjectName $projectName `
        -CommandLine "database:init --connectionString=$connectionString2 --provider SqlServer2008"
    }

function global:UpdateConfig( [string]$electionId )
    {
    $private:config = Get-Config( $electionId )
    
    RunApplicationProject `
        -ProjectName $projectName `
        -CommandLine "update-config --connectionString=$connectionString2 --config=$config"
    }
    
function global:Web( [bool]$underDebugger )
    {
    RunWebApplicationProject `
        -ProjectName $webProjectName `
    }
    
function global:SendData()
    {
    RunApplicationProject `
        -ProjectName $projectName `
        -CommandLine "database:send-data --connectionString=$connectionString --destination=$url2"
    }
    
function global:Get-Config( [Parameter(Mandatory=$true)][string] $electionId )
    {
    $private:configDir = Split-Path -Parent ( ( Get-Project $projectName ).FullName )
    return '"' + ( Join-Path $configDir "$electionId.config" )  + '"'
    }
    
function global:Get-ElectionRootUrl( [Parameter(Mandatory=$true)][string] $electionId )
    {
    switch ( $electionId.ToLower() )
        {
        "pres2008" { return '"http://www.vybory.izbirkom.ru/region/region/izbirkom?action=show&root=1&tvd=100100022249920&vrn=100100022176412&region=0&global=1&sub_region=0&prver=0&pronetvd=null&vibid=100100022249920&type=226"' }
        "pres2012" { return '"http://www.vybory.izbirkom.ru/region/region/izbirkom?action=show&root=1&tvd=100100031793509&vrn=100100031793505&region=0&global=1&sub_region=0&prver=0&pronetvd=null&vibid=100100031793509&type=226"' }
        }
    }

    