# 🚀 Quick-Start Deployment Guide
## LegalConnect → Hostinger Ubuntu 24.04

**Time to deploy:** ~2 hours | **Difficulty:** Intermediate

---

## 📋 Before You Start

**You'll need:**
- Hostinger VPS/shared hosting with Ubuntu 24.04 root SSH access
- Your domain name (registered and pointing to Hostinger)
- GitHub account with your LegalConnect repository
- Google OAuth credentials (Client ID from Google Cloud Console)
- ~30 minutes to read this guide

**Have these ready:**
- Hostinger IP address: `_______________`
- SSH username: `root`
- Your domain: `_______________`
- Google Client ID: `_______________`
- DB password you'll create: `_______________`

---

## 🔧 Phase 1: Server Setup (Hostinger)

**Duration:** ~45 minutes | **Do this once**

### 1. SSH into Hostinger

```bash
ssh root@YOUR_HOSTINGER_IP
# Enter your password when prompted
```

### 2. Copy-Paste This Entire Setup Script

```bash
#!/bin/bash
set -e

echo "=== LegalConnect Server Setup ==="

# Update system
apt update && apt upgrade -y

# Install dependencies
apt install -y curl wget gnupg apt-transport-https ca-certificates lsb-release ubuntu-keyring nginx certbot python3-certbot-nginx

# Install .NET 8 Runtime
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
apt update
apt install -y dotnet-runtime-8.0
rm packages-microsoft-prod.deb

# Install SQL Server
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/24.04/mssql-server-2022.list)" -y
apt update
apt install -y mssql-server

# Install SQL Server tools
add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/24.04/mssql-tools18.list)" -y
apt update
apt install -y mssql-tools18

echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> /etc/profile.d/mssql-tools.sh
source /etc/profile.d/mssql-tools.sh

# Create application directory
mkdir -p /home/legalconnect/{api,wwwroot,logs,uploads/case-documents}
chown -R www-data:www-data /home/legalconnect

# Enable services
systemctl enable nginx
systemctl enable mssql-server

echo "=== Setup complete! ==="
echo ""
echo "NEXT STEPS:"
echo "1. Configure SQL Server: /opt/mssql/bin/mssql-conf setup"
echo "2. Create database (see next section)"
echo "3. Set up SSL certificate"
```

### 3. Run SQL Server Configuration

```bash
/opt/mssql/bin/mssql-conf setup
```

**When prompted:**
- Accept license: `Yes`
- Edition: Choose `Developer` (free, full features)
- SA password: Create a strong password (you'll need it for database setup)

**Wait for:** "Setup has completed successfully"

### 4. Create Database

```bash
# Set your SA password as environment variable (replace with your password)
SA_PASSWORD="YourSAPasswordHere123!@#"

# Connect and create database
sqlcmd -S localhost -U sa -P "$SA_PASSWORD" <<EOF
CREATE DATABASE LegalConnect;
GO

CREATE LOGIN LegalConnectUser WITH PASSWORD = 'YourDbPassword123!@#';
GO

USE LegalConnect;
GO

CREATE USER LegalConnectUser FOR LOGIN LegalConnectUser;
GO

ALTER ROLE db_owner ADD MEMBER LegalConnectUser;
GO

EXIT
EOF

echo "✓ Database created"
```

**Save these credentials:**
```
Database: LegalConnect
User: LegalConnectUser
Password: YourDbPassword123!@#
```

### 5. Set Up SSL Certificate

```bash
# Replace yourdomain.com with YOUR domain
certbot certonly --nginx -d yourdomain.com -d www.yourdomain.com -d api.yourdomain.com

# When prompted:
# - Email: your@email.com
# - Agree to terms: Y
# - Marketing emails: N (your choice)
```

✓ Certificate paths:
- Public: `/etc/letsencrypt/live/yourdomain.com/fullchain.pem`
- Private: `/etc/letsencrypt/live/yourdomain.com/privkey.pem`

---

## ⚙️ Phase 2: Configure Application Files (Your Computer)

**Duration:** ~20 minutes | **Do this on your local machine**

### 1. Update API Configuration

Edit `LegalConnect.API/appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=LegalConnect;User Id=LegalConnectUser;Password=YourDbPassword123!@#;Encrypt=true;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Key": "GENERATE_NEW_KEY_SEE_STEP_2",
    "Issuer": "https://api.yourdomain.com",
    "Audience": "https://yourdomain.com",
    "ExpiryMinutes": "1440"
  },
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com"
  },
  "AllowedHosts": "yourdomain.com,www.yourdomain.com,api.yourdomain.com",
  "Cors": {
    "AllowedOrigins": ["https://yourdomain.com", "https://www.yourdomain.com"]
  }
}
```

### 2. Generate Secure JWT Key

```bash
# On Windows (PowerShell)
[Convert]::ToBase64String((1..32 | ForEach-Object { [byte](Get-Random -Maximum 256) }))

# On Mac/Linux
openssl rand -base64 32
```

Copy the output and paste into `appsettings.Production.json` as the `Jwt.Key` value.

### 3. Update Client Configuration

Edit `LegalConnect.Client/wwwroot/appsettings.Production.json`:

```json
{
  "ApiBaseUrl": "https://api.yourdomain.com/api/",
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com"
  }
}
```

### 4. Generate SSH Key for GitHub Actions

```bash
# Create SSH key (no passphrase!)
ssh-keygen -t rsa -b 4096 -f hostinger_deploy_key -N ""

# Show private key (copy the full output)
cat hostinger_deploy_key
```

**Add public key to Hostinger:**

```bash
# Copy the public key to clipboard
cat hostinger_deploy_key.pub

# On Hostinger server:
mkdir -p /root/.ssh
echo "PASTE_PUBLIC_KEY_HERE" >> /root/.ssh/authorized_keys
chmod 700 /root/.ssh
chmod 600 /root/.ssh/authorized_keys
```

### 5. Set Up GitHub Secrets

In your GitHub repository:

1. **Settings** → **Secrets and variables** → **Actions**
2. **New repository secret** - Add these three:

| Name | Value |
|------|-------|
| `HOSTINGER_HOST` | Your Hostinger IP address |
| `HOSTINGER_USER` | `root` |
| `HOSTINGER_SSH_KEY` | Full contents of `hostinger_deploy_key` (with `-----BEGIN...` and `-----END...`) |

---

## 🚀 Phase 3: Initial Deployment (Manual)

**Duration:** ~15 minutes | **Do this once to test**

### 1. Build on Local Machine

```bash
cd C:\Project\Lawyer_Claude

# Build API
dotnet publish LegalConnect.API/LegalConnect.API.csproj -c Release -o publish/api

# Build Client
dotnet publish LegalConnect.Client/LegalConnect.Client.csproj -c Release -o publish/client
```

### 2. Copy to Hostinger

```bash
# Stop current API (if running)
ssh root@YOUR_HOSTINGER_IP "systemctl stop legalconnect-api"

# Copy files
scp -r publish/api root@YOUR_HOSTINGER_IP:/home/legalconnect/
scp -r publish/client/wwwroot root@YOUR_HOSTINGER_IP:/home/legalconnect/

# Set permissions
ssh root@YOUR_HOSTINGER_IP "chown -R www-data:www-data /home/legalconnect"
```

### 3. Create Systemd Service (Hostinger)

Create file `/etc/systemd/system/legalconnect-api.service`:

```bash
cat > /etc/systemd/system/legalconnect-api.service <<'EOF'
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
EOF

systemctl daemon-reload
systemctl enable legalconnect-api
systemctl start legalconnect-api
systemctl status legalconnect-api
```

### 4. Configure Nginx (Hostinger)

Create file `/etc/nginx/sites-available/legalconnect`:

```nginx
# HTTP redirect to HTTPS
server {
    listen 80;
    server_name yourdomain.com www.yourdomain.com;
    return 301 https://$server_name$request_uri;
}

# HTTPS Server (Client + API)
server {
    listen 443 ssl http2;
    server_name yourdomain.com www.yourdomain.com api.yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    root /home/legalconnect/wwwroot;
    index index.html;

    # Client app routing
    location / {
        try_files $uri $uri/ /index.html;
        expires -1;
        add_header Cache-Control "no-cache, no-store, must-revalidate";
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

    access_log /home/legalconnect/logs/access.log;
    error_log /home/legalconnect/logs/error.log;
}
```

Enable and reload:
```bash
ln -s /etc/nginx/sites-available/legalconnect /etc/nginx/sites-enabled/
nginx -t
systemctl reload nginx
```

### 5. Verify It Works

```bash
# Check API
curl https://yourdomain.com/api/health

# Check Client
curl https://yourdomain.com
```

If you see HTML/JSON responses, you're good! ✓

---

## 🔄 Phase 4: GitHub Actions CI/CD (Automated Deployments)

**Duration:** ~5 minutes | **Do this once**

### 1. Commit Configuration Changes

```bash
cd C:\Project\Lawyer_Claude

git add .github/workflows/
git add LegalConnect.API/appsettings.Production.json
git add LegalConnect.Client/wwwroot/appsettings.Production.json
git commit -m "Add production configuration and GitHub Actions deployment"
git push origin main
```

### 2. GitHub Actions Will Auto-Run

Check **Actions** tab in GitHub:
- ✓ `build` job: Compiles and tests
- ✓ `publish` job: Creates deployment packages
- ✓ `deploy` job: SSHes to Hostinger and deploys
- ✓ `verify` job: Checks if API and client are running

**If deployment fails:**

1. Click the failed job to see logs
2. Fix the issue
3. Push again to trigger re-run

---

## ✅ Verification Checklist

After deployment, verify everything works:

```bash
# On Hostinger or locally:

# 1. Check API is running
curl https://yourdomain.com/api/health
# Expected: 200 OK

# 2. Check Client loads
curl https://yourdomain.com
# Expected: HTML with LegalConnect content

# 3. Check database connection
ssh root@YOUR_HOSTINGER_IP
systemctl status legalconnect-api
journalctl -u legalconnect-api -n 20
# Look for no connection errors

# 4. Check database exists
sqlcmd -S localhost -U LegalConnectUser -P 'YourDbPassword123!@#' -d LegalConnect
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;
GO
EXIT
```

---

## 🐛 Troubleshooting

### API won't start
```bash
# Check logs
journalctl -u legalconnect-api -f

# Common issues:
# - Database password wrong in appsettings.Production.json
# - Port 5001 already in use
# - .NET runtime not installed

# Fix: Restart
systemctl restart legalconnect-api
systemctl status legalconnect-api
```

### Client can't connect to API
```bash
# Check Nginx config
nginx -t

# Check API responds locally
curl http://localhost:5001/api/health

# Check firewall allows 443
ss -tlnp | grep 443

# Fix: Reload Nginx
systemctl reload nginx
```

### GitHub Actions deployment fails
1. Check **Actions** tab for error messages
2. Verify SSH credentials (HOSTINGER_SSH_KEY format)
3. Verify API is accessible: `ssh root@HOST "systemctl status legalconnect-api"`

### Database won't connect
```bash
# Verify SQL Server is running
systemctl status mssql-server

# Verify user exists
sqlcmd -S localhost -U sa -P 'YourSAPassword' -Q "SELECT * FROM sys.sql_logins;"

# Verify database exists
sqlcmd -S localhost -U sa -P 'YourSAPassword' -Q "SELECT * FROM sys.databases WHERE name='LegalConnect';"

# Restart SQL Server
systemctl restart mssql-server
```

---

## 🔐 Security Best Practices

After deployment:

- [ ] Change SA password (stronger than default)
- [ ] Change database user password
- [ ] Regenerate JWT key (done in config)
- [ ] Set up regular backups
- [ ] Enable firewall (only allow 22, 80, 443)
- [ ] Monitor logs for suspicious activity
- [ ] Keep SSL certificate auto-renewing

---

## 📊 Monitoring

### View Logs

```bash
# API logs (last 50 lines, follow)
journalctl -u legalconnect-api -n 50 -f

# Nginx access
tail -f /home/legalconnect/logs/access.log

# Nginx errors
tail -f /home/legalconnect/logs/error.log

# SQL Server
tail -f /var/opt/mssql/log/errorlog
```

### Check Resources

```bash
# Disk space
df -h

# Memory
free -h

# Running processes
ps aux | grep dotnet
```

---

## 🔄 Automatic Updates

GitHub Actions now automatically:
1. Pulls latest code from `main` branch
2. Builds API and Client
3. Runs tests
4. Deploys to Hostinger
5. Verifies health

**Workflow triggers on:**
- Push to `main` branch
- Manual trigger via GitHub Actions

---

## 📝 Next Steps

1. **Done Phase 1-3?** Your site is live at `https://yourdomain.com` ✓
2. **Set up automatic backups** (see DEPLOYMENT_GUIDE.md)
3. **Configure monitoring** (optional: Slack notifications)
4. **Test all features**: Login, create case, upload document, etc.

---

## 🆘 Need Help?

**Common issues:**
- See full DEPLOYMENT_GUIDE.md for detailed troubleshooting
- Check DATABASE_MIGRATION_GUIDE.md if database has issues
- See GITHUB_ACTIONS_SETUP.md for CI/CD problems

**Your notes:**
- Hostinger IP: _______________
- Domain: _______________
- DB password: _______________
- JWT key: _______________

---

## 📞 Support Resources

- **Hostinger Support**: Account dashboard → Support
- **Let's Encrypt**: https://letsencrypt.org/docs/
- **SQL Server on Linux**: https://learn.microsoft.com/en-us/sql/linux/
- **ASP.NET Core on Linux**: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx
- **GitHub Actions**: https://docs.github.com/en/actions

---

**You're ready to deploy!** 🚀

Next: SSH into Hostinger and start with Phase 1.

