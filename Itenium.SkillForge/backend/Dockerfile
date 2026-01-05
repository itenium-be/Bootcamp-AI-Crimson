FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ARG NUGET_USER
ARG NUGET_TOKEN

# Configure NuGet authentication for GitHub Packages
RUN dotnet nuget add source https://nuget.pkg.github.com/Itenium-Forge/index.json \
    --name itenium \
    --username ${NUGET_USER} \
    --password ${NUGET_TOKEN} \
    --store-password-in-clear-text

# Copy project files
COPY nuget.config ./
COPY Itenium.SkillForge.sln ./
COPY Itenium.SkillForge.WebApi/*.csproj ./Itenium.SkillForge.WebApi/

# Restore dependencies
RUN dotnet restore

# Copy source code and build
COPY . .
RUN dotnet publish ./Itenium.SkillForge.WebApi/Itenium.SkillForge.WebApi.csproj -c Release -o /app --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app .
EXPOSE 8080

ENTRYPOINT ["dotnet", "Itenium.SkillForge.WebApi.dll"]
