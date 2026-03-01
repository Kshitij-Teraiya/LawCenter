# LegalConnect Deployment Guide
## Hostinger Ubuntu 24.04 with SQL Server & GitHub Actions

**Version**: 1.0 | **Date**: 2026-03-01 | **Target**: Production

---

## 📋 Table of Contents
1. [Architecture Overview](#architecture-overview)
2. [Pre-Deployment Checklist](#pre-deployment-checklist)
3. [Server Setup (Hostinger Ubuntu 24.04)](#server-setup)
4. [Database Setup (SQL Server on Linux)](#database-setup)
5. [Application Configuration](#application-configuration)
6. [GitHub Actions CI/CD](#github-actions-cicd)
7. [Deployment Process](#deployment-process)
8. [Post-Deployment Verification](#post-deployment-verification)
9. [Troubleshooting](#troubleshooting)

---

## Architecture Overview

```
┌─────────────────┐
│   GitHub Repo   │  (Push to main)
└────────┬────────┘
         │
         ▼
┌─────────────────────────────────────┐
│   GitHub Actions (CI/CD Pipeline)   │
│  ✓ Build API & Client                │
│  ✓ Run tests                         │
│  ✓ Publish artifacts                 │
│  ✓ SSH deploy to server              │
└────────┬────────────────────────────┘
         │
         ▼
┌──────────────────────────────────────────────┐
│      Hostinger Ubuntu 24.04 Server           │
│  ┌────────────────────────────────────────┐  │
│  │  Nginx (Reverse Proxy)                 │  │
│  │  ├─ legalconnect.yourdomain.com ──────┼──┼─────────────────┐
│  │  └─ api.legalconnect.yourdomain.com ──┼──┼──┐              │
│  └────────────────────────────────────────┘  │  │              │
│  ┌────────────────────────────────────────┐  │  │              │
│  │  ASP.NET Core 8 API (Port 5001)        │◄─┘  │              │
│  │  ├─ /api/* endpoints                   │     │              │
│  │  ├─ JWT Authentication                 │     │              │
│  │  └─ SQL Server Connection              │     │              │
│  └──────────────────┬─────────────────────┘    │              │
│                     │                          │              │
│  ┌────────────────────────────────────────┐    │              │
│  │  Blazor WASM Client (Port 5000)        │◄───┘              │
│  │  ├─ Static files served by ASP.NET     │                  │
│  │  ├─ HttpClient calls to API            │                  │
│  │  └─ Client-side routing                │                  │
│  └────────────────────────────────────────┘                  │
│                                                               │
│  ┌────────────────────────────────────────┐                  │
│  │  SQL Server 2022 for Linux             │◄─────────────────┘
│  │  ├─ LegalConnect database              │
│  │  ├─ Users, Profiles, Cases, Deals      │
│  │  └─ Connection via EF Core             │
│  └────────────────────────────────────────┘
│                                             │
│  ┌────────────────────────────────────────┐│
│  │  SSL/TLS (Let's Encrypt)               ││
│  │  ├─ Auto-renewal via Certbot           ││
│  │  └─ https:// for all traffic           ││
│  └────────────────────────────────────────┘│
└──────────────────────────────────────────────┘
```

---

## Pre-Deployment Checklist

- [ ] Have Hostinger account with Ubuntu 24.04 VPS/shared hosting
- [ ] Your domain purchased and pointing to Hostinger nameservers
- [ ] GitHub repository set to private/public (your preference)
- [ ] SSH key pair generated for Hostinger authentication
- [ ] SQL Server 2022 license (or SQL Server Express for free Linux version - 10GB limit)
- [ ] .NET 8 SDK available locally for final testing
- [ ] Backup of current LocalDB database (if you have data to migrate)

---

## Server Setup

### Step 1: Initial Ubuntu 24.04 Setup

**SSH into your Hostinger server:**
```bash
ssh root@your_hostinger_ip
```

**Update system:**
```bash
apt update && apt upgrade -y
```

**Install dependencies:**
```bash
apt install -y curl wget gnupg apt-transport-https ca-certificates lsb-release ubuntu-keyring
```

### Step 2: Install .NET 8 Runtime

```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
apt update

# Install .NET 8 runtime (not SDK - SDK only needed for building)
apt install -y dotnet-runtime-8.0

# Verify installation
dotnet --version
```

### Step 3: Install SQL Server 2022 for Linux

```bash
# Import SQL Server GPG key
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | apt-key add -

# Add SQL Server repository
add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/24.04/mssql-server-2022.list)"
apt update

# Install SQL Server
apt install -y mssql-server

# Run setup (follow prompts for SA password and edition)
/opt/mssql/bin/mssql-conf setup
```

**Important**: When prompted, choose **Developer Edition** (free, full features) or **Express** (free, 10GB limit).

**Verify SQL Server is running:**
```bash
systemctl status mssql-server
```

### Step 4: Install SQL Server Tools (for database management)

```bash
add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/24.04/mssql-tools18.list)"
apt update
apt install -y mssql-tools18

# Add tools to PATH
echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> /etc/profile.d/mssql-tools.sh
source /etc/profile.d/mssql-tools.sh
```

### Step 5: Install Nginx (Reverse Proxy)

```bash
apt install -y nginx

# Enable and start Nginx
systemctl enable nginx
systemctl start nginx
```

### Step 6: Install Certbot for SSL

```bash
apt install -y certbot python3-certbot-nginx

# Create certificate (replace yourdomain.com with your actual domain)
certbot certonly --nginx -d yourdomain.com -d api.yourdomain.com -d www.yourdomain.com
```

---

## Database Setup

### Create LegalConnect Database

```bash
# Connect to SQL Server
sqlcmd -S localhost -U sa -P 'YourSAPassword'

# Execute in sqlcmd prompt:
CREATE DATABASE LegalConnect;
GO

# Create DB user (not using sa for app)
USE LegalConnect;
GO

CREATE LOGIN LegalConnectUser WITH PASSWORD = 'YourDbPassword123!';
GO

CREATE USER LegalConnectUser FOR LOGIN LegalConnectUser;
GO

ALTER ROLE db_owner ADD MEMBER LegalConnectUser;
GO

EXIT
```

**Connection String Format:**
```
Server=localhost,1433;Database=LegalConnect;User Id=LegalConnectUser;Password=YourDbPassword123!;Encrypt=true;TrustServerCertificate=true;Connection Timeout=30;
```

### Run EF Core Migrations

On your local machine (or in GitHub Actions):
```bash
cd LegalConnect.API
dotnet ef database update --configuration Release
```

---

## Application Configuration

### Update API appsettings.json

Create `appsettings.Production.json` in `LegalConnect.API`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=LegalConnect;User Id=LegalConnectUser;Password=YourDbPassword123!;Encrypt=true;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Key": "YourSecureJWTKeyAtLeast32CharactersLongHere",
    "Issuer": "https://api.yourdomain.com",
    "Audience": "https://yourdomain.com",
    "ExpirationMinutes": 1440
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": ["https://yourdomain.com", "https://www.yourdomain.com"]
  },
  "Google": {
    "ClientId": "your.google.client.id.apps.googleusercontent.com"
  }
}
```

### Update Client appsettings.Production.json

Create `wwwroot/appsettings.Production.json` in `LegalConnect.Client`:

```json
{
  "ApiBaseUrl": "https://api.yourdomain.com/api/",
  "Google": {
    "ClientId": "your.google.client.id.apps.googleusercontent.com"
  }
}
```

---

## GitHub Actions CI/CD

### Create `.github/workflows/deploy.yml`

```yaml
name: Deploy to Hostinger

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --configuration Release --no-restore

    - name: Run tests
      run: dotnet test --configuration Release --no-build --verbosity normal --logger "trx;LogFileName=test-results.trx"

    - name: Publish API
      run: dotnet publish LegalConnect.API/LegalConnect.API.csproj -c Release -o ./api-publish

    - name: Publish Client
      run: dotnet publish LegalConnect.Client/LegalConnect.Client.csproj -c Release -o ./client-publish

    - name: Deploy to Hostinger via SSH
      if: github.ref == 'refs/heads/main'
      uses: appleboy/ssh-action@master
      with:
        host: ${{ secrets.HOSTINGER_HOST }}
        username: ${{ secrets.HOSTINGER_USER }}
        key: ${{ secrets.HOSTINGER_SSH_KEY }}
        script: |
          cd /home/legalconnect

          # Stop services
          systemctl stop legalconnect-api || true

          # Backup current version
          cp -r api api.backup

          # Download artifacts from GitHub
          wget https://github.com/YOUR_GITHUB_USERNAME/LegalConnect/releases/download/${{ github.sha }}/api-publish.tar.gz
          wget https://github.com/YOUR_GITHUB_USERNAME/LegalConnect/releases/download/${{ github.sha }}/client-publish.tar.gz

          # Extract and deploy
          tar -xzf api-publish.tar.gz -C ./
          tar -xzf client-publish.tar.gz -C ./wwwroot/

          # Restart services
          systemctl start legalconnect-api
          systemctl status legalconnect-api
```

### Configure GitHub Secrets

Go to **Settings > Secrets and Variables > Actions** and add:
- `HOSTINGER_HOST`: Your server IP or domain
- `HOSTINGER_USER`: SSH user (usually `root` or your username)
- `HOSTINGER_SSH_KEY`: Your private SSH key (without passphrase)
- `GOOGLE_CLIENT_ID`: Your Google OAuth client ID

---

## Deployment Process

### Step 1: Prepare Application Directory

```bash
# On Hostinger server
mkdir -p /home/legalconnect
cd /home/legalconnect

# Create subdirectories
mkdir -p api wwwroot logs

# Set permissions
chmod 755 /home/legalconnect
chown -R www-data:www-data /home/legalconnect
```

### Step 2: Configure Systemd Service for API

Create `/etc/systemd/system/legalconnect-api.service`:

```ini
[Unit]
Description=LegalConnect API
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /home/legalconnect/api/LegalConnect.API.dll
WorkingDirectory=/home/legalconnect/api
StandardOutput=journal
StandardError=journal
SyslogIdentifier=legalconnect-api
User=www-data
Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="ASPNETCORE_URLS=http://localhost:5001"

Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

**Enable the service:**
```bash
systemctl daemon-reload
systemctl enable legalconnect-api
systemctl start legalconnect-api
systemctl status legalconnect-api
```

### Step 3: Configure Nginx

Replace `/etc/nginx/sites-available/legalconnect`:

```nginx
# Redirect HTTP to HTTPS
server {
    listen 80;
    server_name yourdomain.com www.yourdomain.com api.yourdomain.com;
    return 301 https://$server_name$request_uri;
}

# Main HTTPS server (Blazor WASM Client)
server {
    listen 443 ssl http2;
    server_name yourdomain.com www.yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    root /home/legalconnect/wwwroot;
    index index.html;

    # Blazor WASM routing
    location / {
        try_files $uri $uri/ /index.html;
    }

    # API proxy
    location /api/ {
        proxy_pass http://localhost:5001;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Static files cache
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # Logs
    access_log /home/legalconnect/logs/access.log;
    error_log /home/legalconnect/logs/error.log;
}

# API subdomain (optional, if you want api.yourdomain.com)
server {
    listen 443 ssl http2;
    server_name api.yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;

    location / {
        proxy_pass http://localhost:5001;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto https;
    }

    access_log /home/legalconnect/logs/api-access.log;
    error_log /home/legalconnect/logs/api-error.log;
}
```

**Enable Nginx site:**
```bash
ln -s /etc/nginx/sites-available/legalconnect /etc/nginx/sites-enabled/
nginx -t
systemctl restart nginx
```

### Step 4: First Manual Deployment

Publish locally first:

```bash
# Build API
dotnet publish LegalConnect.API/LegalConnect.API.csproj -c Release -o ./publish/api

# Build Client
dotnet publish LegalConnect.Client/LegalConnect.Client.csproj -c Release -o ./publish/client
```

Copy to server:
```bash
scp -r ./publish/api root@your_hostinger_ip:/home/legalconnect/
scp -r ./publish/client/wwwroot/* root@your_hostinger_ip:/home/legalconnect/wwwroot/
```

Start service:
```bash
systemctl start legalconnect-api
```

---

## Post-Deployment Verification

### Test API Endpoint
```bash
curl https://api.yourdomain.com/api/health
```

### Test Client Load
```bash
curl https://yourdomain.com
```

### Check Service Status
```bash
systemctl status legalconnect-api
systemctl status mssql-server
systemctl status nginx
```

### View Logs
```bash
# API logs
journalctl -u legalconnect-api -f

# SQL Server logs
tail -f /var/opt/mssql/log/errorlog

# Nginx logs
tail -f /home/legalconnect/logs/access.log
tail -f /home/legalconnect/logs/error.log
```

---

## Troubleshooting

### SQL Server Connection Refused
```bash
# Check SQL Server status
systemctl status mssql-server

# Verify listening on port 1433
ss -tlnp | grep 1433

# Restart SQL Server
systemctl restart mssql-server
```

### API not accessible from Nginx
```bash
# Check if API is running
curl http://localhost:5001/api/health

# Check nginx error log
tail -f /home/legalconnect/logs/error.log
```

### SSL Certificate Issues
```bash
# Renew certificate
certbot renew --dry-run

# Force renewal
certbot renew --force-renewal
```

### Database Migration Failed
```bash
# Rollback locally and test
dotnet ef database update -c ApplicationDbContext <previous-migration-name>

# Then retry migration
dotnet ef database update
```

---

## Maintenance

### Auto-renewal of SSL Certificates
Certbot automatically renews every 60 days. Verify with:
```bash
certbot renew --dry-run
```

### Backup Database
```bash
# Create backup
sqlcmd -S localhost -U sa -P 'YourSAPassword' -Q "BACKUP DATABASE LegalConnect TO DISK = '/var/opt/mssql/backup/legalconnect.bak'"
```

### Monitor Disk Space
```bash
df -h
du -sh /home/legalconnect
du -sh /var/opt/mssql
```

---

**Next Steps:**
1. Follow Server Setup section (Steps 1-6)
2. Follow Database Setup section
3. Update application configuration
4. Set up GitHub Actions secrets
5. First manual deployment for testing
6. Enable automated deployments via GitHub Actions

