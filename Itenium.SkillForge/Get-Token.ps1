# Get-Token.ps1
# Helper script to get an access token for testing the API

param(
    [string]$Username = "backoffice",
    [string]$Password = "AdminPassword123!",
    [string]$BaseUrl = "http://localhost:5000"
)

$body = @{
    grant_type = "password"
    username = $Username
    password = $Password
    client_id = "skillforge-spa"
    scope = "openid profile email"
}

try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/connect/token" -Method Post -Body $body -ContentType "application/x-www-form-urlencoded"

    Write-Host "Access Token:" -ForegroundColor Green
    Write-Host $response.access_token
    Write-Host ""
    Write-Host "Expires in: $($response.expires_in) seconds" -ForegroundColor Yellow

    # Copy to clipboard
    $response.access_token | Set-Clipboard
    Write-Host "Token copied to clipboard!" -ForegroundColor Cyan
}
catch {
    Write-Host "Error getting token: $_" -ForegroundColor Red
}
