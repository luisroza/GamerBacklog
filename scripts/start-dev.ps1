$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$env:DOTNET_CLI_HOME = Join-Path $root ".dotnet-home"
$env:APPDATA = Join-Path $root ".appdata"
$env:LOCALAPPDATA = Join-Path $root ".localappdata"

dotnet run --project "src\GameBacklog.Web\GameBacklog.Web.csproj" --no-build --urls "http://localhost:5188"
