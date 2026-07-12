# Deploying to Raspberry Pi

## Prerequisites

### On the Raspberry Pi:
1. Install .NET 8 ASP.NET runtime:
   ```bash
   sudo apt update
   sudo apt install -y aspnetcore-runtime-8.0
   ```

2. Ensure SSH is enabled (usually is by default on Raspberry Pi OS)

### On your Windows machine:
- .NET 8 SDK (already installed if you're building the project)
- SSH client (built into Windows 10/11)
- SSH key access to the Pi (recommended — run `ssh-copy-id pi@<pi-ip>` from WSL or use PuTTYgen)

## Deploy

From the project root, run:

```powershell
.\deploy\deploy-pi.ps1 -PiHost "192.168.1.100" -PiUser "pi"
```

Or with custom port:
```powershell
.\deploy\deploy-pi.ps1 -PiHost "batby-pi.local" -PiUser "pi" -AppPort 8080
```

This will:
1. Build the app for Linux ARM64
2. Stop any running instance on the Pi
3. Copy the files to `/opt/batby-education/`
4. Install a systemd service
5. Start the app

## Access

After deployment, access the app from any device on the network:
```
http://<pi-ip>:5000
```

## Management

```bash
# Check if running
ssh pi@<pi-ip> 'sudo systemctl status batby-education'

# View live logs
ssh pi@<pi-ip> 'sudo journalctl -u batby-education -f'

# Restart after deploy
ssh pi@<pi-ip> 'sudo systemctl restart batby-education'

# Stop
ssh pi@<pi-ip> 'sudo systemctl stop batby-education'
```

## Database

The SQLite database (`batbyeducation.db`) lives in `/opt/batby-education/`. 

To back it up:
```bash
ssh pi@<pi-ip> 'cp /opt/batby-education/batbyeducation.db /opt/batby-education/batbyeducation.db.bak'
```

## Raspberry Pi 3 (32-bit)

If you're using a Pi 3 or older (armv7), change the runtime in the deploy script from `linux-arm64` to `linux-arm`:

```powershell
# In deploy-pi.ps1, change:
-r linux-arm64
# To:
-r linux-arm
```

## Firewall

If other devices can't connect, ensure the port is open:
```bash
ssh pi@<pi-ip> 'sudo ufw allow 5000/tcp'
```

(Only needed if UFW is enabled — it's usually not on default Raspberry Pi OS.)
