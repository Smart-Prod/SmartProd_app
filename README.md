# SmartProd

Sistema de gestão de produção com controle de estoque, ordens de produção, estruturas de produto (BOM), notas fiscais e relatórios.

## Estrutura

```text
SmartProd_app/
├── SmartProd/
│   ├── SmartProd.Server/       # ASP.NET Core Web API (.NET 10), EF Core e PostgreSQL
│   ├── SmartProd.AppHost/      # Orquestração local com .NET Aspire
│   ├── SmartProd.Tests.Unit/   # Testes unitários da API
│   ├── SmartProd.Tests.Integration/ # Testes de integração da API
│   └── frontend/               # React, TypeScript, Vite e React Query
├── SmartProd_Mobile_Front-end/ # Aplicativo .NET MAUI
├── db-init/                    # Inicialização do PostgreSQL no Docker
└── docker-compose.yml          # PostgreSQL, API e frontend
```

## Executar com Docker Compose

Requer Docker Desktop com Docker Compose v2.

1. Copie o exemplo de configuração e defina senhas/segredos exclusivos:

   ```powershell
   Copy-Item .env.example .env
   ```

   Edite `.env`. `JWT_SECRET` deve ter pelo menos 32 bytes; `ADMIN_INITIAL_PASSWORD` deve ter pelo menos 12 caracteres. O administrador inicial só é criado quando o banco ainda não contém usuários.
2. Inicie os serviços:

   ```powershell
   docker compose up --build
   ```
3. Acesse a interface em `http://localhost` (ou na porta configurada por `FRONTEND_PORT`). A API fica disponível em `http://localhost:5481` por padrão.

As migrações do Entity Framework são aplicadas na inicialização da API. Os dados do PostgreSQL persistem no volume `smartprod-postgres`. O seed de demonstração (`POST /api/v1/Seed/kaixote`) exige papel Admin e só opera em uma base sem produtos.

## Executar localmente

Requer .NET 10 SDK, Node.js 20+, npm e PostgreSQL 15. Crie um banco PostgreSQL e configure as variáveis de ambiente no terminal do backend. O ASP.NET Core não carrega arquivos `.env` diretamente; para desenvolvimento local use variáveis do processo ou `dotnet user-secrets`. O arquivo `.env` da raiz é carregado pelo Docker Compose.

Exemplo PowerShell:

```powershell
$env:DB_CONNECTION_STRING = "Host=localhost;Port=5432;Database=SmartProd;Username=postgres;Password=<sua-senha>"
$env:JWT_SECRET = "<segredo-aleatorio-com-pelo-menos-32-bytes>"
$env:ADMIN_INITIAL_EMAIL = "admin@localhost"
$env:ADMIN_INITIAL_PASSWORD = "<senha-inicial-com-pelo-menos-12-caracteres>"
$env:CORS_ORIGINS = "http://localhost:5173"
dotnet run --project SmartProd\SmartProd.Server\SmartProd.Server.csproj
```

Em outro terminal:

```powershell
Set-Location SmartProd\frontend
npm ci
npm run dev
```

O Vite encaminha `/api` para a API local em `http://localhost:5481`. Para apontar o frontend a outro servidor, configure `VITE_API_URL` no ambiente de build. Para exemplos adicionais de configuração, consulte [CONFIGURATION.md](./SmartProd/SmartProd.Server/CONFIGURATION.md).

## Verificações

```powershell
dotnet build SmartProd\SmartProd.Server\SmartProd.Server.csproj
dotnet test SmartProd\SmartProd.Tests.Unit\SmartProd.Tests.Unit.csproj
dotnet test SmartProd\SmartProd.Tests.Integration\SmartProd.Tests.Integration.csproj
Set-Location SmartProd\frontend
npm run build
npm run lint
npm run test -- --run
```

GitHub Actions executa os pipelines de backend e frontend em pushes e pull requests para `main`. O workflow combinado também verifica os builds de Docker.

## Observabilidade e exportações

- `GET /health` verifica a disponibilidade da aplicação e suas dependências; `GET /health/ready` verifica se ela está pronta, incluindo a conexão PostgreSQL.
- Para exportar traces, defina `OTEL_EXPORTER_OTLP_ENDPOINT` para o endpoint de um coletor OTLP. A API instrumenta ASP.NET Core, `HttpClient`, runtime e consultas Npgsql.
- As rotas autenticadas `/api/v1/Relatorio/produtos.csv`, `/api/v1/Relatorio/movimentacoes.csv`, `/api/v1/Relatorio/estoque.pdf` e `/api/v1/Relatorio/boms.pdf` baixam os relatórios correspondentes.

## Rotas

As rotas da API usam o prefixo `/api/v1`. Login e registro ficam em `/api/v1/Usuario/login` e `/api/v1/Usuario/register`; a renovação rotativa de sessão fica em `/api/v1/auth/refresh`. Os demais controladores exigem autenticação. A administração de usuários e o seed exigem o papel `Admin`.

## Segurança

- Não adicione `.env`, senhas, JWTs ou connection strings reais ao Git.
- A autenticação usa JWT; refresh tokens são opacos, armazenados somente como hash e rotacionados a cada uso.
- O primeiro administrador é provisionado a partir de `ADMIN_INITIAL_EMAIL` e `ADMIN_INITIAL_PASSWORD` apenas se não houver usuários cadastrados.
- O seed de demonstração não apaga registros existentes e só pode ser executado por um administrador.
- Consulte [SECURITY.md](./SECURITY.md) para reportar vulnerabilidades.

## Licença

MIT — consulte [LICENSE](./LICENSE).
