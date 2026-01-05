# Debug Docker build and run for SkillForge backend
# Run from backend folder

# Clean up old containers
docker rm -f skillforge-postgres 2>$null
docker rm -f postgres 2>$null

# Build the image (--no-cache to ensure new appsettings.Docker.json is included)
docker build --no-cache --build-arg NUGET_USER=$env:NUGET_USER --build-arg NUGET_TOKEN=$env:NUGET_TOKEN -t skillforge-test .

# Start postgres (from parent folder)
Push-Location ..
docker compose up -d
Pop-Location

# Find the network that postgres is on
$network = docker inspect postgres --format '{{range $key, $value := .NetworkSettings.Networks}}{{$key}}{{end}}'
Write-Host "Using network: $network"

# Run backend on same network as postgres
# Uses DOTNET_ENVIRONMENT=Docker -> appsettings.Docker.json (Host=postgres)
docker run --rm -it -p 8080:8080 --network $network skillforge-test
