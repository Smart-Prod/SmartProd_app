# SmartProd - Smart Production Management System

[![CI](https://github.com/Smart-Prod/SmartProd_app/actions/workflows/ci.yml/badge.svg)](https://github.com/Smart-Prod/SmartProd_app/actions/workflows/ci.yml)
[![Security Scan](https://github.com/Smart-Prod/SmartProd_app/actions/workflows/ci.yml/badge.svg?event=security-scan)](https://github.com/Smart-Prod/SmartProd_app/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

SmartProd is a comprehensive production management system for manufacturing operations, featuring inventory control, production orders, bill of materials, fiscal notes, and real-time dashboards.

## Architecture

```
SmartProd_app/
├── SmartProd/
│   ├── SmartProd.Server/          # ASP.NET Core 8 Web API
│   │   ├── Controllers/           # API endpoints
│   │   ├── Services/              # Business logic
│   │   ├── Models/                # Entity Framework entities
│   │   ├── DTOs/                  # Data transfer objects
│   │   └── Data/                  # DbContext & migrations
│   ├── SmartProd.AppHost/         # .NET Aspire orchestration
│   └── frontend/                  # React 18 + TypeScript + Vite
│       ├── src/
│       │   ├── components/        # Reusable UI components
│       │   ├── contexts/          # React Context providers
│       │   ├── pages/             # Page components
│       │   ├── services/          # API client
│       │   └── utils/             # Helpers & validators
│       └── package.json
├── SmartProd_Mobile_Front-end/    # .NET MAUI mobile app
├── .github/                       # GitHub Actions, templates, configs
└── docs/                          # Documentation
```

## Features

- **Inventory Management**: Raw materials (MP) & finished goods (PA) with stock levels, reservations, minimums
- **Production Orders**: Plan, track, and complete production with BOM consumption
- **Bill of Materials**: Define product recipes with material quantities
- **Stock Movements**: Track all entradas, saídas, produção, consumo with full audit trail
- **Fiscal Notes**: Entrada/saída notes with automatic stock updates (transactional)
- **Real-time Dashboard**: KPIs, charts, alerts for low stock
- **Reports**: Production efficiency, stock levels, sales, material consumption
- **Authentication**: JWT-based with roles (Admin, Operator, Owner)
- **Mobile App**: .NET MAUI for shop floor operations

## Quick Start

### Prerequisites
- .NET 8 SDK
- Node.js 20+
- PostgreSQL 15+

### Development

```bash
# 1. Clone
git clone https://github.com/Smart-Prod/SmartProd_app.git
cd SmartProd_app

# 2. Backend setup
cd SmartProd/SmartProd.Server
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=SmartProd;Username=postgres;Password=your_password"
dotnet user-secrets set "Jwt:Secret" "$(openssl rand -base64 64)"

# 3. Database (Docker)
docker run -d --name smartprod-db \
  -e POSTGRES_DB=SmartProd \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=your_password \
  -p 5432:5432 postgres:15

# 4. Run backend
dotnet run

# 5. Run frontend (new terminal)
cd SmartProd/frontend
npm install
npm run dev
```

### Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `SMARTPROD_DB_CONNECTION` | PostgreSQL connection string | Yes |
| `SMARTPROD_JWT_SECRET` | JWT signing key (64+ chars) | Yes |
| `SMARTPROD_JWT_ISSUER` | JWT issuer (default: SmartProd) | No |
| `SMARTPROD_JWT_EXPIRES_IN_DAYS` | Token expiry (default: 7) | No |

See [CONFIGURATION.md](SmartProd/SmartProd.Server/CONFIGURATION.md) for details.

## API Endpoints

| Module | Endpoints |
|--------|-----------|
| **Auth** | `POST /api/Usuario/register`, `POST /api/Usuario/login` |
| **Products** | `GET/POST /api/Produto`, `GET /api/Produto/{id}`, `GET /api/Produto/{code}/qrcode` |
| **Production Orders** | `GET/POST /api/ProductionOrder`, `PUT /api/ProductionOrder/{id}` |
| **Stock Movements** | `GET /api/Movimentacao`, `POST /api/Movimentacao/consumo`, `POST /api/Movimentacao/finalizar` |
| **Fiscal Notes** | `GET/POST /api/NotaFiscal` |
| **BOM** | `GET/POST /api/BOM` |
| **Dashboard** | `GET /api/Dashboard` |
| **Reports** | `GET /api/Relatorio/producao`, `GET /api/Relatorio/consumo-mp` |
| **Seed (Admin)** | `POST /api/Seed/kaixote` |

## Deployment

### Docker Compose (Local/Staging)
```bash
docker-compose up -d
```

### Kubernetes (Production)
```bash
kubectl apply -f k8s/production/
```

### GitHub Actions CI/CD
- **CI**: Runs on every push/PR (tests, lint, security scan, Docker build)
- **CD**: Manual or tag-triggered deployments to staging/production
- **Environments**: `staging` (auto from develop), `production` (manual approval)

## Security

- All API endpoints require JWT authentication (except login/register)
- Role-based access control (Admin, Operator, Owner)
- Seed endpoint protected with Admin role
- Secrets managed via environment variables/GitHub Secrets
- Dependency scanning via Dependabot + Trivy

See [SECURITY.md](SECURITY.md) for vulnerability reporting.

## Contributing

1. Read [CONTRIBUTING.md](CONTRIBUTING.md)
2. Check [good first issues](https://github.com/Smart-Prod/SmartProd_app/labels/good%20first%20issue)
3. Follow [branch protection rules](.github/BRANCH_PROTECTION.md)
4. Use conventional commits

## License

MIT License - see [LICENSE](LICENSE) for details.

## Support

- **Issues**: [GitHub Issues](https://github.com/Smart-Prod/SmartProd_app/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Smart-Prod/SmartProd_app/discussions)
- **Security**: [SECURITY.md](SECURITY.md)

---

**Organization**: [Smart-Prod](https://github.com/Smart-Prod)  
**Maintainers**: @Smart-Prod/maintainers