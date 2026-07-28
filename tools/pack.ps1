# Usage:
#   .\pack.ps1				# stable package, e.g. 5.0.0
#   .\pack.ps1 -VersionSuffix preview.1	# prerelease package, e.g. 5.0.0-preview.1
#
# From .cmd:
#   pack.cmd
#   pack.cmd -VersionSuffix preview.1

param(
    [string]$VersionSuffix = ""
)

$csproj = "$PSScriptRoot\..\Photino.NET\PhotinoX.csproj"
$Configuration = "Release"
$outDir = $PSScriptRoot

dotnet clean $csproj -c $Configuration

if ([string]::IsNullOrWhiteSpace($VersionSuffix)) {
    dotnet pack $csproj -c $Configuration -o $outDir
}
else {
    dotnet pack $csproj -c $Configuration -o $outDir -p:VersionSuffix=$VersionSuffix
}