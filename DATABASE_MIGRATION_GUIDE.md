# Database Migration Guide
## LocalDB → SQL Server 2022 for Linux

---

## Overview

This guide covers:
1. **Exporting data** from your local SQL Server LocalDB
2. **Creating database** on production SQL Server Linux
3. **Migrating tables and data** using EF Core migrations
4. **Verifying** data integrity post-migration

---

## Prerequisites

- [ ] LocalDB instance with LegalConnectDb running
- [ ] SQL Server 2022 for Linux installed on Hostinger server
- [ ] SQL Server tools (sqlcmd) installed locally
- [ ] .NET 8 SDK installed locally
- [ ] Access to LegalConnect.API source code

---

## Scenario 1: Fresh Start (No Data Migration)

**Use if:** You don't have production data in LocalDB, or want to start fresh.

### Step 1: Create Database on Production Server

```bash
# On Hostinger server
sqlcmd -S localhost -U sa -P 'YourSAPassword'
```

In sqlcmd prompt:
```sql
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
```

### Step 2: Run EF Core Migrations

On your local machine (or in GitHub Actions before deployment):

```bash
cd C:\Project\Lawyer_Claude\LegalConnect.API

# Set connection string for production
$env:ASPNETCORE_ENVIRONMENT = "Production"

# Apply all migrations to production database
# WARNING: This applies to the production DB - verify connection string first!
dotnet ef database update --configuration Release
```

### Step 3: Verify Database

```bash
# On Hostinger server
sqlcmd -S localhost -U LegalConnectUser -P 'YourDbPassword123!@#' -d LegalConnect

# In sqlcmd:
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE';
GO

-- Check specific tables
SELECT COUNT(*) FROM AspNetUsers;
GO

SELECT COUNT(*) FROM ClientProfiles;
GO

EXIT
```

---

## Scenario 2: Migrate Existing Data

**Use if:** You have data in LocalDB that you want to keep.

### Step 1: Backup LocalDB

```bash
# On Windows, backup your LocalDB
sqlcmd -S (localdb)\mssqllocaldb -d LegalConnectDb -Q "BACKUP DATABASE LegalConnectDb TO DISK = 'C:\Backups\LegalConnectDb.bak'"
```

### Step 2: Export Data from LocalDB

```bash
# Generate SQL script with data
# Option A: Using SSMS (SQL Server Management Studio)
# - Right-click database → Tasks → Generate Scripts
# - Choose "Script data" option
# - Select all tables and save to file

# Option B: Using sqlcmd script
sqlcmd -S (localdb)\mssqllocaldb -d LegalConnectDb -Q "sp_MSForEachTable 'SELECT * FROM ?'" -o export.sql
```

### Step 3: Create Production Database

```bash
# On Hostinger server
sqlcmd -S localhost -U sa -P 'YourSAPassword'
```

```sql
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
```

### Step 4: Run EF Migrations First

Before importing data, set up the schema:

```bash
cd C:\Project\Lawyer_Claude\LegalConnect.API

# Apply migrations to production server
# Update appsettings.Production.json with SQL Server connection first!

$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet ef database update --configuration Release
```

### Step 5: Import Data Using SQL Script

```bash
# Copy export file to server
scp export.sql root@hostinger:/home/legalconnect/

# On Hostinger server
sqlcmd -S localhost -U LegalConnectUser -P 'YourDbPassword123!@#' -d LegalConnect -i export.sql
```

### Step 6: Verify Data

```bash
sqlcmd -S localhost -U LegalConnectUser -P 'YourDbPassword123!@#' -d LegalConnect

-- Verify row counts match
SELECT 'AspNetUsers' AS TableName, COUNT(*) AS RecordCount FROM AspNetUsers
UNION ALL
SELECT 'ClientProfiles', COUNT(*) FROM ClientProfiles
UNION ALL
SELECT 'LawyerProfiles', COUNT(*) FROM LawyerProfiles
UNION ALL
SELECT 'Cases', COUNT(*) FROM Cases
UNION ALL
SELECT 'HireRequests', COUNT(*) FROM HireRequests
UNION ALL
SELECT 'Deals', COUNT(*) FROM Deals
GO

EXIT
```

---

## Scenario 3: Using Backup & Restore

**Use if:** You have a backup file (.bak) from LocalDB.

### Step 1: Transfer Backup File

```bash
# Copy .bak file from Windows to Hostinger
scp C:\Backups\LegalConnectDb.bak root@hostinger:/var/opt/mssql/backup/
```

### Step 2: Restore Database

```bash
# On Hostinger server
sqlcmd -S localhost -U sa -P 'YourSAPassword'
```

```sql
-- Restore from backup
RESTORE DATABASE LegalConnect
FROM DISK = '/var/opt/mssql/backup/LegalConnectDb.bak'
GO

-- Verify restore
SELECT * FROM sys.databases WHERE name = 'LegalConnect'
GO

-- Update login/user if needed
CREATE LOGIN LegalConnectUser WITH PASSWORD = 'YourDbPassword123!@#';
GO

USE LegalConnect;
GO

CREATE USER LegalConnectUser FOR LOGIN LegalConnectUser;
GO

ALTER ROLE db_owner ADD MEMBER LegalConnectUser;
GO

EXIT
```

### Step 3: Verify & Apply Any Pending Migrations

```bash
# From local machine
cd LegalConnect.API

$env:ASPNETCORE_ENVIRONMENT = "Production"

# Check for pending migrations
dotnet ef migrations list

# Apply any pending migrations (this is safe if data already exists)
dotnet ef database update --configuration Release
```

---

## Connection String Formats

### LocalDB (Development)
```
Server=(localdb)\mssqllocaldb;Database=LegalConnectDb;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False;TrustServerCertificate=True;
```

### SQL Server Linux (Production)
```
Server=localhost,1433;Database=LegalConnect;User Id=LegalConnectUser;Password=YourDbPassword123!@#;Encrypt=true;TrustServerCertificate=true;Connection Timeout=30;MultipleActiveResultSets=true;
```

---

## Important Considerations

### Identity/Primary Keys
- EF Core handles identity column preservation
- If manually inserting data, respect identity values
- Use `SET IDENTITY_INSERT` carefully:
```sql
SET IDENTITY_INSERT Users ON;
-- INSERT statements with explicit IDs
SET IDENTITY_INSERT Users OFF;
```

### Foreign Keys
- Ensure all parent records exist before child records
- Disable FK constraints during import if needed:
```sql
ALTER TABLE Deals NOCHECK CONSTRAINT ALL;
-- Import data
ALTER TABLE Deals WITH CHECK CHECK CONSTRAINT ALL;
```

### Dates & Timestamps
- LocalDB and SQL Server handle datetime similarly
- Verify timezone-aware columns are correct post-import
- Check `CreatedAt`, `UpdatedAt` timestamps

### Google OAuth & JWT
- User tokens from LocalDB won't work on production (different JWT key)
- All users will need to re-authenticate
- This is normal and expected

---

## Rollback Plan

If something goes wrong during migration:

### Option 1: Restore from Backup
```bash
# On Hostinger
sqlcmd -S localhost -U sa -P 'YourSAPassword'
```

```sql
USE master;
GO

ALTER DATABASE LegalConnect SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

DROP DATABASE LegalConnect;
GO

-- Restore from backup
RESTORE DATABASE LegalConnect
FROM DISK = '/var/opt/mssql/backup/LegalConnectDb.bak'
GO

ALTER DATABASE LegalConnect SET MULTI_USER;
GO
```

### Option 2: Rollback to Previous API Version
If API changes caused issues:

```bash
# On Hostinger
cd /home/legalconnect
systemctl stop legalconnect-api
cp -r api.backup.<timestamp> api
systemctl start legalconnect-api
```

---

## Verification Checklist

After migration, verify:

- [ ] Database exists: `SELECT * FROM sys.databases`
- [ ] Tables exist: `SELECT * FROM INFORMATION_SCHEMA.TABLES`
- [ ] Data matches row counts
- [ ] Identity/PK values correct
- [ ] FK relationships intact
- [ ] Application connects successfully
- [ ] Login/registration works
- [ ] All user data accessible
- [ ] Case and deal data present
- [ ] No orphaned records

---

## Post-Migration Steps

1. **Update appsettings.Production.json** on server with correct connection string
2. **Restart API service**: `systemctl restart legalconnect-api`
3. **Test all major features**:
   - Login with existing user
   - Create new user
   - Create case/deal
   - Upload document
   - Google OAuth login
4. **Monitor logs** for any errors: `journalctl -u legalconnect-api -f`
5. **Backup production DB**: See [Maintenance](#maintenance) section

---

## EF Core Migration Troubleshooting

### "Connection string was not found"
```bash
# Ensure ASPNETCORE_ENVIRONMENT is set
$env:ASPNETCORE_ENVIRONMENT = "Production"

# Verify appsettings.Production.json exists
Get-Item LegalConnect.API\appsettings.Production.json
```

### "Login failed for user 'LegalConnectUser'"
```bash
# Check user exists on server
sqlcmd -S localhost -U sa -P 'YourSAPassword'
SELECT * FROM sys.sql_logins WHERE name = 'LegalConnectUser';
GO
```

### "Database does not exist"
```bash
# Create it before running migrations
sqlcmd -S localhost -U sa -P 'YourSAPassword' -Q "CREATE DATABASE LegalConnect;"
```

### "Table already exists" during migration
- Migrations are idempotent (safe to re-run)
- If you get this error, data might be partially loaded
- Use `dotnet ef database update` without target migration to complete

---

## Database Maintenance

### Regular Backups

```bash
# On Hostinger, create backup script
#!/bin/bash
BACKUP_DIR="/var/opt/mssql/backup"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

sqlcmd -S localhost -U sa -P 'YourSAPassword' -Q "BACKUP DATABASE LegalConnect TO DISK = '$BACKUP_DIR/legalconnect_$TIMESTAMP.bak'"

# Keep only last 7 days of backups
find $BACKUP_DIR -name "legalconnect_*.bak" -mtime +7 -delete
```

Schedule with cron:
```bash
0 2 * * * /home/legalconnect/backup-db.sh
```

### Index Maintenance

```sql
-- Rebuild fragmented indexes
ALTER INDEX ALL ON AspNetUsers REBUILD;
GO

DBCC SHRINKDATABASE(LegalConnect, 10);
GO
```

