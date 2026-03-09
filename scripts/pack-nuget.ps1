param(
    [string]$Config = "scripts/nuget.publish.json",
    [switch]$Push,
    [string]$ApiKey
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Invoke-Step {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [scriptblock]$Action
    )

    Write-Host "==> $Name" -ForegroundColor Cyan
    & $Action
}

function Add-PackProperty {
    param(
        [Parameter(Mandatory = $true)]
        [System.Collections.Generic.List[string]]$Args,
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [AllowNull()]
        [AllowEmptyString()]
        [string]$Value
    )

    if (-not [string]::IsNullOrWhiteSpace($Value)) {
        $escapedValue = $Value.Replace(";", "%3B")
        $Args.Add("/p:$Name=$escapedValue") | Out-Null
    }
}

if (-not (Test-Path $Config)) {
    throw "Config file not found: $Config"
}

$configData = Get-Content -Path $Config -Raw | ConvertFrom-Json

$project = [string]$configData.project
$configuration = if ($configData.configuration) { [string]$configData.configuration } else { "Release" }
$output = if ($configData.output) { [string]$configData.output } else { "artifacts/nuget" }
$skipBuild = [bool]$configData.skipBuild

if ([string]::IsNullOrWhiteSpace($project)) {
    throw "'project' is required in config."
}

if (-not (Test-Path $project)) {
    throw "Project file not found: $project"
}

New-Item -ItemType Directory -Path $output -Force | Out-Null

Invoke-Step -Name "Restore" -Action {
    dotnet restore $project
}

if (-not $skipBuild) {
    Invoke-Step -Name "Build" -Action {
        dotnet build $project -c $configuration --no-restore
    }
}

$packArgs = [System.Collections.Generic.List[string]]::new()
$packArgs.AddRange([string[]]("pack", $project, "-c", $configuration, "-o", $output, "--no-restore"))

if (-not $skipBuild) {
    $packArgs.Add("--no-build") | Out-Null
}

if ($configData.package) {
    Add-PackProperty -Args $packArgs -Name "PackageId" -Value ([string]$configData.package.id)
    Add-PackProperty -Args $packArgs -Name "Version" -Value ([string]$configData.package.version)
    Add-PackProperty -Args $packArgs -Name "Authors" -Value ([string]$configData.package.authors)
    Add-PackProperty -Args $packArgs -Name "Company" -Value ([string]$configData.package.company)
    Add-PackProperty -Args $packArgs -Name "Description" -Value ([string]$configData.package.description)
    Add-PackProperty -Args $packArgs -Name "PackageTags" -Value ([string]$configData.package.tags)
    Add-PackProperty -Args $packArgs -Name "PackageProjectUrl" -Value ([string]$configData.package.packageProjectUrl)
    Add-PackProperty -Args $packArgs -Name "RepositoryUrl" -Value ([string]$configData.package.repositoryUrl)
    Add-PackProperty -Args $packArgs -Name "RepositoryType" -Value ([string]$configData.package.repositoryType)

    if ($null -ne $configData.package.includeSymbols) {
        Add-PackProperty -Args $packArgs -Name "IncludeSymbols" -Value ([string]([bool]$configData.package.includeSymbols).ToString().ToLowerInvariant())
    }

    Add-PackProperty -Args $packArgs -Name "SymbolPackageFormat" -Value ([string]$configData.package.symbolPackageFormat)
}

Invoke-Step -Name "Pack" -Action {
    $packArgArray = $packArgs.ToArray()
    dotnet @packArgArray
}

$packages = Get-ChildItem -Path $output -Filter "*.nupkg" |
    Where-Object { $_.Name -notlike "*.symbols.nupkg" }

if (-not $packages) {
    throw "No .nupkg file generated in $output"
}

Write-Host "Generated package(s):" -ForegroundColor Green
$packages | ForEach-Object { Write-Host "  $($_.FullName)" }

$pushEnabled = [bool]$configData.push.enabled
if ($Push) {
    $pushEnabled = $true
}

if ($pushEnabled) {
    $source = if ($configData.push.source) { [string]$configData.push.source } else { "https://api.nuget.org/v3/index.json" }
    $skipDuplicate = if ($null -ne $configData.push.skipDuplicate) { [bool]$configData.push.skipDuplicate } else { $true }
    $apiKeyEnvVar = if ($configData.push.apiKeyEnvVar) { [string]$configData.push.apiKeyEnvVar } else { "NUGET_API_KEY" }

    if ([string]::IsNullOrWhiteSpace($ApiKey)) {
        $ApiKey = [Environment]::GetEnvironmentVariable($apiKeyEnvVar)
    }

    if ([string]::IsNullOrWhiteSpace($ApiKey)) {
        throw "NuGet API key is missing. Set -ApiKey or environment variable '$apiKeyEnvVar'."
    }

    foreach ($pkg in $packages) {
        Invoke-Step -Name "Push $($pkg.Name)" -Action {
            $pushArgs = @("nuget", "push", $pkg.FullName, "--api-key", $ApiKey, "--source", $source)
            if ($skipDuplicate) {
                $pushArgs += "--skip-duplicate"
            }

            dotnet @pushArgs
        }
    }
}

Write-Host "Done." -ForegroundColor Green
