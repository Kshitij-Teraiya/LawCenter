# GitHub Actions Setup Guide

## Configure Repository Secrets

GitHub Actions CI/CD pipeline requires secrets to authenticate with your Hostinger server.

### Step 1: Generate SSH Key (if not already done)

**On your local machine:**

```bash
ssh-keygen -t rsa -b 4096 -f hostinger_deploy_key -N ""
```

This creates:
- `hostinger_deploy_key` - Private key (keep secret)
- `hostinger_deploy_key.pub` - Public key (upload to server)

### Step 2: Add Public Key to Hostinger Server

```bash
# On Hostinger server
mkdir -p /root/.ssh
cat hostinger_deploy_key.pub >> /root/.ssh/authorized_keys
chmod 700 /root/.ssh
chmod 600 /root/.ssh/authorized_keys
```

### Step 3: Configure GitHub Secrets

1. Go to GitHub repository **Settings**
2. Click **Secrets and variables** → **Actions**
3. Click **New repository secret** and add each of these:

#### Required Secrets:

| Secret Name | Value | Example |
|------------|-------|---------|
| `HOSTINGER_HOST` | Your Hostinger server IP or hostname | `123.45.67.89` or `your-server.hostinger.com` |
| `HOSTINGER_USER` | SSH username (usually `root`) | `root` |
| `HOSTINGER_SSH_KEY` | Private key contents (full text of `hostinger_deploy_key`) | `-----BEGIN RSA PRIVATE KEY-----...` |

#### Optional Secrets:

| Secret Name | Value | Purpose |
|------------|-------|---------|
| `SLACK_WEBHOOK` | Slack webhook URL | Get notified of deployment status |

### Step 4: Add Secrets via CLI (Alternative)

```bash
# Install GitHub CLI
# https://cli.github.com/

# Login to GitHub
gh auth login

# Add secrets
gh secret set HOSTINGER_HOST --body "123.45.67.89"
gh secret set HOSTINGER_USER --body "root"
gh secret set HOSTINGER_SSH_KEY --body "$(cat hostinger_deploy_key)"
```

---

## Update Configuration Files

Before first deployment, update these files with your actual values:

### 1. appsettings.Production.json (API)

**File:** `LegalConnect.API/appsettings.Production.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=LegalConnect;User Id=LegalConnectUser;Password=YOUR_DB_PASSWORD_HERE;..."
  },
  "Jwt": {
    "Key": "YOUR_SECURE_JWT_KEY_AT_LEAST_32_CHARACTERS_LONG_HERE!",
    "Issuer": "https://api.yourdomain.com",
    "Audience": "https://yourdomain.com",
    ...
  },
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com"
  },
  "AllowedHosts": "yourdomain.com,www.yourdomain.com,api.yourdomain.com",
  ...
}
```

**Required changes:**
- `Password`: Database password from server setup
- `Key`: Generate with: `openssl rand -base64 32`
- `Issuer/Audience`: Your production domain
- `ClientId`: From Google OAuth console
- `AllowedHosts`: Your domain(s)

### 2. appsettings.Production.json (Client)

**File:** `LegalConnect.Client/wwwroot/appsettings.Production.json`

```json
{
  "ApiBaseUrl": "https://api.yourdomain.com/api/",
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com"
  }
}
```

**Required changes:**
- `ApiBaseUrl`: Your production API domain
- `ClientId`: Same Google ID as API

### 3. Deploy Workflow (if needed)

**File:** `.github/workflows/deploy-to-hostinger.yml`

Search and replace:
- `yourdomain.com` → Your actual domain
- `api.yourdomain.com` → Your API subdomain

---

## Push Changes to GitHub

```bash
# Navigate to project directory
cd C:\Project\Lawyer_Claude

# Stage all files
git add .

# Commit changes
git commit -m "Add deployment configuration for Hostinger"

# Push to main branch
git push origin main
```

---

## Monitor Deployments

### In GitHub

1. Go to repository **Actions** tab
2. Click on workflow run to view logs
3. Each job shows detailed output:
   - `build`: Compilation and tests
   - `publish`: Artifact creation
   - `deploy`: SSH deployment
   - `verify`: Health checks

### View Workflow Logs

```bash
# Using GitHub CLI
gh run view <run-id> --log

# Or view in browser:
# https://github.com/YOUR_USERNAME/LegalConnect/actions
```

---

## First Deployment Checklist

Before pushing to `main` and triggering the workflow:

- [ ] Hostinger Ubuntu 24.04 server provisioned
- [ ] .NET 8 runtime installed on server
- [ ] SQL Server 2022 installed and running
- [ ] LegalConnect database created
- [ ] Nginx installed and configured
- [ ] SSL certificate installed (Let's Encrypt)
- [ ] SSH key added to server authorized_keys
- [ ] GitHub secrets configured (HOSTINGER_HOST, HOSTINGER_USER, HOSTINGER_SSH_KEY)
- [ ] appsettings.Production.json files updated with real values
- [ ] Systemd service file created: `/etc/systemd/system/legalconnect-api.service`
- [ ] Application directories created: `/home/legalconnect/{api,wwwroot,logs,uploads}`
- [ ] Directory permissions set correctly

---

## Manual Testing Before Automation

Test SSH deployment script manually first:

```bash
# On local machine
# Test SSH connection
ssh -i hostinger_deploy_key root@123.45.67.89

# On server, test service management
systemctl status legalconnect-api
systemctl restart legalconnect-api

# Test API endpoint
curl http://localhost:5001/api/health
```

---

## Troubleshooting GitHub Actions

### Workflow doesn't start
- Check if file is valid YAML: `.github/workflows/deploy-to-hostinger.yml`
- Must be in `main` branch
- Check for syntax errors in file

### SSH connection fails
- Verify `HOSTINGER_HOST` is correct IP/hostname
- Verify `HOSTINGER_USER` is correct (usually `root`)
- Verify SSH key has newlines: use full key text with `-----BEGIN...` and `-----END...`

### Deployment fails but SSH works
- Check systemd service is created: `systemctl status legalconnect-api`
- Check API logs: `journalctl -u legalconnect-api -f`
- Check Nginx config: `nginx -t`
- Check database connection: `sqlcmd -S localhost -U sa`

### Verify step fails
- Update domain in workflow file to match your actual domain
- Check firewall allows HTTPS (443)
- Check SSL certificate is valid: `certbot certificates`

---

## Environment-Specific Configuration

### Local Development
- Uses `appsettings.json` (LocalDB)
- Google ClientId: Your dev OAuth app ID
- Jwt.Key: Development key

### Production (Hostinger)
- Uses `appsettings.Production.json` (SQL Server Linux)
- Google ClientId: Your production OAuth app ID
- Jwt.Key: Secure production key
- Environment variable: `ASPNETCORE_ENVIRONMENT=Production`

---

## Secrets Best Practices

✓ **Do:**
- Keep secrets in GitHub Secrets, not in code
- Rotate secrets periodically
- Use strong, unique values (especially JWT key and DB password)
- Store SSH keys securely, never commit them
- Use limited-scope SSH keys when possible

✗ **Don't:**
- Commit appsettings.Production.json to GitHub (use template only)
- Hardcode secrets in workflow files
- Use same secrets across environments
- Share SSH keys via email or chat

---

## Continuous Deployment Options

### Option 1: Auto-deploy on push to main
Current setup - workflow triggers automatically

```yaml
on:
  push:
    branches: [ main ]
```

### Option 2: Manual trigger only
Only deploy when you explicitly trigger:

```yaml
on:
  workflow_dispatch:
```

### Option 3: Scheduled deployments
Deploy on a schedule (e.g., nightly):

```yaml
on:
  schedule:
    - cron: '0 2 * * *'  # 2 AM daily
```

---

## Database Migrations in CI/CD

If you want automatic database migrations during deployment:

1. Add to workflow after starting API:
```bash
# In deploy script
cd /home/legalconnect/api
/usr/bin/dotnet LegalConnect.API.dll --migrate
```

2. Or manually before deployment:
```bash
dotnet ef database update --project LegalConnect.API -c ApplicationDbContext
```

---

## Rollback Procedures

If deployment fails and you need to rollback:

```bash
# On Hostinger server
cd /home/legalconnect

# Stop current API
systemctl stop legalconnect-api

# List backups (sorted by newest first)
ls -t api.backup.* | head -5

# Restore previous version
rm -rf api
cp -r api.backup.1234567890 api

# Start API
systemctl start legalconnect-api

# Verify
systemctl status legalconnect-api
```

Workflow includes automatic backup and rollback on deployment failure.

