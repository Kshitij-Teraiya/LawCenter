# 🎯 LegalConnect Deployment to Hostinger
## Ubuntu 24.04 | SQL Server | GitHub Actions CI/CD

---

## 📚 Documentation Map

Complete deployment guides have been created for your project. Here's what each document covers:

### 🚀 **START HERE: [QUICKSTART_DEPLOYMENT.md](./QUICKSTART_DEPLOYMENT.md)** (20 mins)
**For:** Getting your site live ASAP
- Pre-flight checklist
- Phase 1: Server setup (copy-paste scripts)
- Phase 2: Configuration updates
- Phase 3: Initial manual deployment
- Phase 4: Automated CI/CD setup
- Verification checklist
- Quick troubleshooting

→ **Read this first and follow step-by-step**

---

## 📖 Detailed Reference Guides

### [DEPLOYMENT_GUIDE.md](./DEPLOYMENT_GUIDE.md) (Comprehensive)
**For:** Deep understanding of the deployment architecture
- Complete system architecture diagram
- Pre-deployment checklist
- Step-by-step server setup (all 6 phases)
- SQL Server 2022 for Linux installation
- Application configuration details
- GitHub Actions workflow explanation
- Nginx reverse proxy configuration
- SSL/TLS with Let's Encrypt
- Post-deployment verification
- Comprehensive troubleshooting guide
- Maintenance procedures

→ **Read when you need detailed explanations or encounter issues**

---

### [GITHUB_ACTIONS_SETUP.md](./GITHUB_ACTIONS_SETUP.md) (CI/CD)
**For:** Configuring automated deployments
- SSH key generation
- GitHub secrets configuration
- Configuration file updates
- First deployment checklist
- Manual testing procedures
- Troubleshooting CI/CD failures
- Rollback procedures
- Environment-specific configurations

→ **Read when setting up GitHub Actions automation**

---

### [DATABASE_MIGRATION_GUIDE.md](./DATABASE_MIGRATION_GUIDE.md) (Data)
**For:** Migrating from LocalDB to SQL Server Linux
- Three migration scenarios:
  - Fresh start (no data)
  - Migrate existing data
  - Restore from backup
- EF Core migration process
- Backup and restore procedures
- Verification checklists
- Troubleshooting migration issues
- Post-migration steps
- Maintenance (backups, indexes)

→ **Read if you have existing data in LocalDB to migrate**

---

## 📁 New Files Created

### Application Configuration Files
```
LegalConnect.API/
├── appsettings.Production.json    ← NEW (Edit with your values)

LegalConnect.Client/wwwroot/
├── appsettings.Production.json    ← NEW (Edit with your values)
```

**What these files do:**
- `appsettings.Production.json` (API): Production database connection, JWT secrets, Google OAuth, Kestrel port
- `appsettings.Production.json` (Client): Production API URL, Google OAuth client ID

**What you need to do:**
1. Open each file
2. Replace placeholder values with your actual production values
3. Commit to GitHub

---

### CI/CD Automation
```
.github/workflows/
└── deploy-to-hostinger.yml        ← NEW (GitHub Actions workflow)
```

**What it does:**
- Runs on every push to `main` branch
- Builds and tests your project
- Publishes API and Client packages
- SSHes into Hostinger and deploys
- Verifies the deployment succeeded

**What you need to do:**
1. Generate SSH keys for Hostinger
2. Add GitHub secrets (HOSTINGER_HOST, HOSTINGER_USER, HOSTINGER_SSH_KEY)
3. Push to main branch
4. Watch **Actions** tab for automated deployment

---

## 🎓 Step-by-Step Overview

### **Day 1: Server Setup (1-2 hours)**

1. **[QUICKSTART_DEPLOYMENT.md](./QUICKSTART_DEPLOYMENT.md) - Phase 1** (45 mins)
   - SSH into Hostinger
   - Run setup script
   - Configure SQL Server
   - Create database
   - Set up SSL certificate

2. **[QUICKSTART_DEPLOYMENT.md](./QUICKSTART_DEPLOYMENT.md) - Phase 2** (20 mins)
   - Update `appsettings.Production.json` files
   - Generate JWT key
   - Add SSH key to Hostinger
   - Configure GitHub secrets

---

### **Day 1: Initial Deployment (30 mins)**

3. **[QUICKSTART_DEPLOYMENT.md](./QUICKSTART_DEPLOYMENT.md) - Phase 3** (15 mins)
   - Build project locally
   - Copy files to Hostinger
   - Create systemd service
   - Configure Nginx
   - Verify it works

---

### **Day 2: Automate with GitHub Actions (10 mins)**

4. **[QUICKSTART_DEPLOYMENT.md](./QUICKSTART_DEPLOYMENT.md) - Phase 4** (5 mins)
   - Commit config changes
   - GitHub Actions auto-runs
   - Push new code → Auto-deploy
   - Monitor in Actions tab

---

### **Optional: Data Migration (30 mins)**

5. **[DATABASE_MIGRATION_GUIDE.md](./DATABASE_MIGRATION_GUIDE.md)**
   - If you have existing data in LocalDB
   - Choose appropriate scenario (Fresh/Migrate/Restore)
   - Follow step-by-step instructions
   - Verify data integrity

---

## 🔑 Key Deployment Information

### Technology Stack
```
┌─ Hostinger Ubuntu 24.04 Server
│  ├─ Nginx (Reverse Proxy / Static Files)
│  ├─ ASP.NET Core 8 (API, Port 5001)
│  ├─ Blazor WASM (Client, Port 5000)
│  └─ SQL Server 2022 for Linux (Database, Port 1433)
│
├─ GitHub Actions (CI/CD Pipeline)
│  ├─ Build, test, publish on every push
│  ├─ SSH deploy to Hostinger
│  └─ Health check verification
│
└─ SSL/TLS (Let's Encrypt)
   ├─ Auto-renewal (every 60 days)
   └─ Full HTTPS for all traffic
```

### Deployment Flow
```
Push to main branch
        ↓
GitHub Actions builds project
        ↓
Runs tests
        ↓
Publishes API & Client
        ↓
SSHes to Hostinger
        ↓
Stops API service
        ↓
Backups current version
        ↓
Deploys new version
        ↓
Starts API service
        ↓
Health check verification
        ↓
Ready for users! ✓
```

---

## ⚡ Quick Reference: Configuration Values You'll Need

### On Hostinger (after Phase 1 setup)
| Setting | Where to Find | Example |
|---------|---------------|---------|
| IP Address | Hostinger control panel | `123.45.67.89` |
| SSH Username | Usually `root` | `root` |
| SA Password | You create in `/opt/mssql/bin/mssql-conf setup` | `YourSAPass123!@#` |
| DB User Password | You create in setup script | `YourDbPass123!@#` |
| Database Name | You create in setup script | `LegalConnect` |
| SSL Certificate | Certbot creates after running | `/etc/letsencrypt/live/yourdomain.com/` |

### In Your Config Files (need to update)
| File | Setting | Example |
|------|---------|---------|
| `appsettings.Production.json` (API) | ConnectionString | `Server=localhost,1433;Database=LegalConnect;User Id=...` |
| `appsettings.Production.json` (API) | Jwt.Key | (Generate with `openssl rand -base64 32`) |
| `appsettings.Production.json` (API) | Jwt.Issuer | `https://api.yourdomain.com` |
| `appsettings.Production.json` (API) | Google.ClientId | Your Google OAuth client ID |
| `appsettings.Production.json` (Client) | ApiBaseUrl | `https://api.yourdomain.com/api/` |
| `appsettings.Production.json` (Client) | Google.ClientId | Same as API |

### In GitHub Secrets (need to add)
| Secret | Value | Where from |
|--------|-------|-----------|
| `HOSTINGER_HOST` | `123.45.67.89` | Hostinger control panel |
| `HOSTINGER_USER` | `root` | SSH username |
| `HOSTINGER_SSH_KEY` | Full private key text | Generated with `ssh-keygen` |

---

## 🎯 Your Deployment Checklist

### ✅ Pre-Deployment
- [ ] Hostinger VPS provisioned (Ubuntu 24.04)
- [ ] Domain registered and pointing to Hostinger
- [ ] Google OAuth credentials obtained
- [ ] GitHub repository ready
- [ ] This project cloned to your machine

### ✅ Phase 1: Server Setup
- [ ] SSH connection to Hostinger working
- [ ] Setup script executed successfully
- [ ] SQL Server installed and running
- [ ] LegalConnect database created
- [ ] Nginx installed
- [ ] SSL certificate installed
- [ ] Directories created (`/home/legalconnect/*`)

### ✅ Phase 2: Configuration
- [ ] `appsettings.Production.json` (API) updated
- [ ] `appsettings.Production.json` (Client) updated
- [ ] JWT key generated and saved
- [ ] SSH key generated and added to Hostinger
- [ ] GitHub secrets configured (3 secrets)

### ✅ Phase 3: Initial Deploy
- [ ] Project builds locally without errors
- [ ] Files copied to Hostinger
- [ ] Systemd service created and running
- [ ] Nginx configured and reloaded
- [ ] API responds at `https://api.yourdomain.com/api/health`
- [ ] Client loads at `https://yourdomain.com`

### ✅ Phase 4: Automation
- [ ] Config changes committed to GitHub
- [ ] GitHub Actions workflow triggered
- [ ] Deploy job completed successfully
- [ ] Verify job passed health checks
- [ ] Next push triggers auto-deployment

---

## 🆘 Need Help?

### Common Issues & Solutions

**"SSH Connection Refused"**
- Verify IP address is correct
- Check Hostinger firewall allows SSH (port 22)
- Verify SSH key has correct permissions: `chmod 600 hostinger_deploy_key`

**"Database Connection Failed"**
- Check SA password is correct
- Verify SQL Server is running: `systemctl status mssql-server`
- Check database exists: `sqlcmd -S localhost -U sa -P 'password'`

**"GitHub Actions Deploy Failed"**
- Check SSH key in GitHub secrets is complete (with BEGIN/END lines)
- Verify HOSTINGER_USER is `root`
- Check HOSTINGER_HOST is IP address, not domain
- Look at failed job logs in GitHub Actions

**"API won't start"**
- Check `appsettings.Production.json` has correct DB password
- Check database exists and is accessible
- View logs: `journalctl -u legalconnect-api -f`

**"Client can't connect to API"**
- Check `appsettings.Production.json` (Client) has correct API URL
- Verify Nginx is proxying `/api/` to `localhost:5001`
- Check firewall allows HTTPS (port 443)

---

## 📚 Document Reading Order

**First Time?** Follow this order:
1. **This file** (DEPLOYMENT_README.md) ← You are here
2. **[QUICKSTART_DEPLOYMENT.md](./QUICKSTART_DEPLOYMENT.md)** - Follow all 4 phases
3. **[GITHUB_ACTIONS_SETUP.md](./GITHUB_ACTIONS_SETUP.md)** - Configure CI/CD
4. **[DEPLOYMENT_GUIDE.md](./DEPLOYMENT_GUIDE.md)** - Deep dive (reference only)

**Troubleshooting?**
- Check [DEPLOYMENT_GUIDE.md](./DEPLOYMENT_GUIDE.md) Troubleshooting section
- Check specific guide for your issue (GitHub Actions, Database, etc.)

**Data Migration?**
- Read [DATABASE_MIGRATION_GUIDE.md](./DATABASE_MIGRATION_GUIDE.md)

---

## 🎯 Project Deployment Success Criteria

After following all guides, you should have:

✓ **Live Website**
- `https://yourdomain.com` loads client
- `https://api.yourdomain.com/api/health` returns JSON
- HTTPS enabled (green lock icon)
- Can login with email or Google
- Can create cases and upload documents

✓ **Automated Deployment**
- Push to `main` branch triggers build
- GitHub Actions workflow completes successfully
- Changes appear on production within 5 minutes
- Rollback available if needed

✓ **Database & Data**
- All user data persists
- Cases and documents searchable
- Database backups available
- No data loss

✓ **Monitoring**
- Can view API logs: `journalctl -u legalconnect-api -f`
- Can check server resources: `df -h`, `free -h`
- SSL certificate auto-renews
- Error logs accessible

---

## 🚀 You're Ready!

Everything you need is documented. Start with:

### **→ [QUICKSTART_DEPLOYMENT.md](./QUICKSTART_DEPLOYMENT.md)**

Follow Phase 1 → Phase 2 → Phase 3 → Phase 4

**Total time to live:** ~2 hours

Questions? Check the specific guide for your issue.

Good luck! 🎉

---

## 📊 Project Stats

**Deployment Guides Created:**
- 4 comprehensive markdown documents
- 1 GitHub Actions workflow
- 2 production configuration files
- 100+ lines of nginx configuration
- 50+ bash/sqlcmd commands

**Topics Covered:**
- Server setup and provisioning
- SQL Server Linux installation
- Database creation and configuration
- ASP.NET Core production setup
- Nginx reverse proxy
- Let's Encrypt SSL/TLS
- GitHub Actions CI/CD
- SSH deployment automation
- Database migration strategies
- Troubleshooting and monitoring

**This is production-ready infrastructure code.** 🎯

