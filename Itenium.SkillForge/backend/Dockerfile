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

# Copy project files for restore
COPY nuget.config ./
COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY Itenium.SkillForge.WebApi/*.csproj ./Itenium.SkillForge.WebApi/
COPY Itenium.SkillForge.Data/*.csproj ./Itenium.SkillForge.Data/
COPY Itenium.SkillForge.Entities/*.csproj ./Itenium.SkillForge.Entities/
COPY Itenium.SkillForge.Services/*.csproj ./Itenium.SkillForge.Services/

# Restore dependencies
RUN dotnet restore ./Itenium.SkillForge.WebApi/Itenium.SkillForge.WebApi.csproj

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
