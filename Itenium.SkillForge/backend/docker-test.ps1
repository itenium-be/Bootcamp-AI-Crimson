# Test Docker build and run for SkillForge backend

# Set your GitHub credentials (or use existing env vars)
# $env:NUGET_USER = "your-github-username"
# $env:NUGET_TOKEN = "your-pat-with-read-packages"

# Build the Docker image
docker build --build-arg NUGET_USER=$env:NUGET_USER --build-arg NUGET_TOKEN=$env:NUGET_TOKEN -t skillforge-test .

# Start PostgreSQL (from project root, run once)
# cd .. && docker compose up -d && cd backend

# Run the backend container (interactive to see logs)
docker run --rm -it -p 8080:8080 --network host -e ASPNETCORE_ENVIRONMENT=Development -e "ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=skillforge;Username=skillforge;Password=skillforge" skillforge-test

# Or run detached and check logs:
# docker run -d --name skillforge-debug -p 8080:8080 --network host -e ASPNETCORE_ENVIRONMENT=Development -e "ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=skillforge;Username=skillforge;Password=skillforge" skillforge-test
# docker logs -f skillforge-debug
# docker rm -f skillforge-debug
