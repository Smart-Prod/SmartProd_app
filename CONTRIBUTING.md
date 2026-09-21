# Contributing to SmartProd

Thank you for your interest in contributing! This guide will help you get started.

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 20+
- PostgreSQL 15+
- Docker (optional, for containerized development)

### Local Development Setup

1. **Fork and clone the repository**
   ```bash
   git clone https://github.com/Smart-Prod/SmartProd_app.git
   cd SmartProd_app
   ```

2. **Configure backend secrets**
   ```bash
   cd SmartProd/SmartProd.Server
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=SmartProd;Username=postgres;Password=your_password"
   dotnet user-secrets set "Jwt:Secret" "$(openssl rand -base64 64)"
   ```

3. **Start PostgreSQL** (using Docker)
   ```bash
   docker run -d \
     --name smartprod-db \
     -e POSTGRES_DB=SmartProd \
     -e POSTGRES_USER=postgres \
     -e POSTGRES_PASSWORD=your_password \
     -p 5432:5432 \
     postgres:15
   ```

4. **Run backend**
   ```bash
   cd SmartProd/SmartProd.Server
   dotnet run
   ```

5. **Run frontend**
   ```bash
   cd SmartProd/frontend
   npm install
   npm run dev
   ```

## Development Workflow

### Branch Naming

| Type | Pattern | Example |
|------|---------|---------|
| Feature | `feature/<short-description>` | `feature/add-product-search` |
| Bug fix | `fix/<short-description>` | `fix/login-token-refresh` |
| Hotfix | `hotfix/<short-description>` | `hotfix/security-jwt-validation` |
| Refactor | `refactor/<short-description>` | `refactor/product-service` |
| Docs | `docs/<short-description>` | `docs/update-api-docs` |

### Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only
- `style`: Formatting, missing semicolons, etc.
- `refactor`: Code change that neither fixes a bug nor adds a feature
- `perf`: Performance improvement
- `test`: Adding missing tests
- `chore`: Maintenance, dependencies, build config

Examples:
```
feat(products): add QR code generation for products
fix(auth): handle expired JWT tokens correctly
docs: update API documentation for v2
refactor(orders): simplify order status transitions
```

### Pull Request Process

1. **Create a feature branch** from `develop`
2. **Make your changes** with tests
3. **Run CI locally** (optional but recommended)
   ```bash
   # Backend
   cd SmartProd/SmartProd.Server
   dotnet test
   
   # Frontend
   cd SmartProd/frontend
   npm run lint && npm run typecheck && npm run test
   ```
4. **Push and open PR** against `develop`
5. **Fill out the PR template** completely
6. **Wait for reviews** (2 approvals required for main)
7. **Address feedback** and push updates
8. **Merge** after all checks pass

## Code Standards

### C# (Backend)

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use `var` when type is obvious
- Prefer expression-bodied members for simple methods
- Use `async`/`await` consistently
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)

### TypeScript/React (Frontend)

- Follow [TypeScript ESLint recommended](https://typescript-eslint.io/rules/)
- Use functional components with hooks
- Prefer `interface` over `type` for object shapes
- Use absolute imports (`@/components/...`)
- Write JSDoc for public APIs

### Database

- Use migrations for all schema changes
- Name migrations descriptively: `AddProductIndex`, `FixUserRoleEnum`
- Test migrations locally before PR

## Testing Requirements

### Backend
- Unit tests for all services
- Integration tests for controllers
- Aim for >80% coverage

### Frontend
- Unit tests for utilities and hooks
- Component tests for complex UI
- E2E tests for critical user flows

## Code Review Guidelines

### As a Reviewer
- Be respectful and constructive
- Focus on correctness, security, performance
- Suggest improvements, don't mandate style preferences
- Approve when confident, request changes when needed

### As an Author
- Keep PRs small (<400 lines when possible)
- Respond to all comments
- Don't take feedback personally
- Update PR description if scope changes

## Security

- **Never commit secrets** - Use user-secrets (dev) or GitHub Secrets (CI)
- Run `dotnet user-secrets list` to verify
- Report security issues via [SECURITY.md](SECURITY.md)

## Documentation

- Update README for user-facing changes
- Update API docs (OpenAPI/Swagger) for endpoint changes
- Add code comments for complex logic
- Update CHANGELOG.md for notable changes

## Release Process

1. **Version bump** in `Directory.Build.props` or `package.json`
2. **Create release branch**: `release/v1.2.0`
3. **Final testing** on staging
4. **Tag release**: `git tag v1.2.0 && git push --tags`
5. **GitHub Actions** builds and deploys automatically
6. **Create GitHub Release** with changelog

## Getting Help

- Check existing issues and PRs
- Read the [documentation](docs/)
- Ask in discussions or Slack
- Tag maintainers: @Smart-Prod/maintainers

## Code of Conduct

By participating, you agree to abide by our [Code of Conduct](CODE_OF_CONDUCT.md).

---

**First time contributing?** Look for issues labeled `good first issue` or `help wanted`!