param ($version, $suffix, $env='release', [switch]$push=$false)

$fullVersion = -join($version, '-', $suffix)
$outFolder = ".\dist\$fullVersion"

dotnet pack .\uSync.Triggers\uSync.Triggers.csproj -c $env -o $outFolder /p:ContinuousIntegrationBuild=true,version=$fullVersion 
dotnet pack .\uSyncTriggerCLI\uSyncTriggerCLI.csproj -c $env -o $outFolder /p:ContinuousIntegrationBuild=true,version=$fullVersion 

if ($push) {
    .\dist\nuget.exe push "$outFolder\*.nupkg" -ApiKey AzureDevOps -src https://pkgs.dev.azure.com/jumoo/Public/_packaging/nightly/nuget/v3/index.json
}