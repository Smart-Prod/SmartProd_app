# SmartProd Server - Configuration Guide

## ⚠️ Security: Never Commit Secrets!

This project uses **environment variables** and **User Secrets** for sensitive configuration.

## Required Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `SMARTPROD_DB_CONNECTION` | PostgreSQL connection string | `Host=localhost;Port=5432;Database=SmartProd;Username=postgres;Password=secure_pass` |
| `SMARTPROD_JWT_SECRET` | JWT signing key (min 32 chars, base64 recommended) | `openssl rand -base64 64` |
| `SMARTPROD_JWT_ISSUER` | JWT issuer (optional, default: SmartProd) | `SmartProd` |
| `SMARTPROD_JWT_EXPIRES_IN_DAYS` | Token expiration in days (optional, default: 7) | `7` |

## Setup Options

### Option 1: Environment Variables (Production/Containers)
```bash
export SMARTPROD_DB_CONNECTION="Host=db;Port=5432;Database=SmartProd;Username=smartprod;Password=xxx"
export SMARTPROD_JWT_SECRET="$(openssl rand -base64 64)"
```

### Option 2: User Secrets (Local Development)
```bash
cd SmartProd/SmartProd.Server
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=SmartProd;Username=postgres;Password=your_pass"
dotnet user-secrets set "Jwt:Secret" "$(openssl rand -base64 64)"
```

### Option 3: .env File (Docker Compose)
```bash
cp .env.example .env
# Edit .env with your values
docker-compose up
```

## Configuration Priority (Highest Wins)
1. Environment Variables
2. User Secrets (Development only)
3. appsettings.Development.json
4. appsettings.json

## Generate JWT Secret
```bash
# Linux/macOS
openssl rand -base64 64

# PowerShell
[Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Maximum 256 }))
```

## Verify Configuration
```bash
cd SmartProd/SmartProd.Server
dotnet run --environment Development
# Check logs for "Configuration loaded" messages
```