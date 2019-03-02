Push-Location

Set-Location -Path "smite-cli"

dotnet pack --configuration Release

Pop-Location
