# Deploy Batby Education to Raspberry Pi
# 
# Prerequisites:
#   - .NET 8 SDK installed on this machine (for building)
#   - SSH access to the Pi (ssh keys recommended)
#   - .NET 8 ASP.NET runtime on the Pi: 
#     sudo apt install -y dotnet-runtime-8.0 aspnetcore-runtime-8.0
#
# Usage:
#   .\deploy\deploy-pi.ps1 -PiHost "192.168.1.100" -PiUser "pi"
#   .\deploy\deploy-pi.ps1 -PiHost "batby-pi.local" -PiUser "pi" -PiPort 22

param(
    [Parameter(Mandatory=$true)]
    [string]$PiHost,
    
    [string]$PiUser = "pi",
    [int]$PiPort = 22,
    [string]$RemotePath = "/opt/batby-education",
    [int]$HttpPort = 80,
    [int]$HttpsPort = 443
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent (Split-Path -Parent $PSCommandPath)
$PublishDir = Join-Path $ProjectRoot "publish-pi"

Write-Host "=== Batby Education - Raspberry Pi Deploy ===" -ForegroundColor Cyan
Write-Host "Target: $PiUser@${PiHost}:$RemotePath"
Write-Host "Ports: HTTP $HttpPort, HTTPS $HttpsPort"
Write-Host ""

# Step 1: Build for ARM64 (Raspberry Pi 4/5) or ARM32 (Pi 3 and older)
Write-Host "[1/5] Publishing for linux-arm64..." -ForegroundColor Yellow
dotnet publish "$ProjectRoot\src\BatbyEducation.Web" `
    -c Release `
    -r linux-arm64 `
    --self-contained false `
    -o $PublishDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "[2/5] Stopping existing service on Pi..." -ForegroundColor Yellow
ssh -p $PiPort "$PiUser@$PiHost" "sudo systemctl stop batby-education 2>/dev/null || true"

Write-Host "[3/5] Copying files to Pi..." -ForegroundColor Yellow
ssh -p $PiPort "$PiUser@$PiHost" "sudo mkdir -p $RemotePath && sudo chown $PiUser $RemotePath"
scp -P $PiPort -r "$PublishDir/*" "${PiUser}@${PiHost}:${RemotePath}/"

Write-Host "[4/5] Setting up systemd service..." -ForegroundColor Yellow
$ServiceFile = @"
[Unit]
Description=Batby Education Tutoring System
After=network.target

[Service]
Type=notify
User=root
WorkingDirectory=$RemotePath
ExecStart=/usr/bin/dotnet $RemotePath/BatbyEducation.Web.dll
Environment=ASPNETCORE_URLS=http://0.0.0.0:$HttpPort;https://0.0.0.0:$HttpsPort
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
AmbientCapabilities=CAP_NET_BIND_SERVICE
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=batby-education

[Install]
WantedBy=multi-user.target
"@

# Write service file via SSH
$ServiceFile | ssh -p $PiPort "$PiUser@$PiHost" "sudo tee /etc/systemd/system/batby-education.service > /dev/null"

Write-Host "[5/5] Starting service..." -ForegroundColor Yellow
ssh -p $PiPort "$PiUser@$PiHost" @"
sudo systemctl daemon-reload
sudo systemctl enable batby-education
sudo systemctl start batby-education
"@

Write-Host ""
Write-Host "=== Deployment complete! ===" -ForegroundColor Green
Write-Host "Access the app at: http://${PiHost}" -ForegroundColor Cyan
Write-Host "  (HTTPS will use a self-signed cert unless you configure one)"
Write-Host ""
Write-Host "Useful commands:"
Write-Host "  Check status: ssh $PiUser@$PiHost 'sudo systemctl status batby-education'"
Write-Host "  View logs:    ssh $PiUser@$PiHost 'sudo journalctl -u batby-education -f'"
Write-Host "  Restart:      ssh $PiUser@$PiHost 'sudo systemctl restart batby-education'"
Write-Host "  Stop:         ssh $PiUser@$PiHost 'sudo systemctl stop batby-education'"

# Cleanup local publish folder
Remove-Item -Recurse -Force $PublishDir
