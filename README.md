c# SmartProd — Sistema de Gestão de Produção Industrial

Sistema full-stack para gestão de produção industrial: cadastro de produtos, fichas técnicas (BOM), ordens de produção, movimentações de estoque, notas fiscais, controle de usuários (Admin/Operador) e relatórios.

## Superfícies

| Superfície | Tecnologia | Público |
|------------|-----------|---------|
| **Mobile** | .NET MAUI (Android/iOS/Windows) | Operadores de chão de fábrica |
| **Web Dashboard** | React 18 + TypeScript + Vite 7 + shadcn/ui | Administradores e gestores |
| **API** | ASP.NET Core (.NET 10) | Central para ambas superfícies |
| **Orquestração** | .NET Aspire | Dev/production deploy |

## Arquitetura

```
SMARTPROD_/
├── SmartProd_Mobile_Front-end.slnx          ← Solução principal
├── SmartProd_Mobile_Front-end/              ← App MAUI
│   ├── MauiProgram.cs                       ← DI, HttpClient → localhost:5481
│   ├── App.xaml.cs                          ← Janela 350×700
│   ├── AppShell.xaml                        ← Navegação Flyout (5 páginas)
│   ├── Models/                              ← DTOs (LoginResponse, OrdemProducao, etc.)
│   ├── Services/ApiService.cs               ← HTTP client para API
│   ├── ViewsModels/                         ← MVVM (5 ViewModels)
│   ├── Views/                               ← Páginas XAML (5 páginas)
│   └── Platforms/Android/AndroidManifest.xml
│
└── SmartProd/
    ├── SmartProd.Server/                    ← API ASP.NET Core
    │   ├── Program.cs                       ← Startup: EF Core, JWT, CORS, DI
    │   ├── Extensions.cs                    ← Aspire service defaults
    │   ├── Data/AppDbContext.cs             ← EF Core (8 DbSets)
    │   ├── Models/                          ← Entidades (10)
    │   ├── DTOs/                            ← Data Transfer Objects (8)
    │   ├── Enum/                            ← Enumerações (6)
    │   ├── Controllers/                     ← Controllers (6)
    │   ├── Services/                        ← Lógica de negócio (6)
    │   └── Utils/ApiResponse.cs
    │
    ├── SmartProd.AppHost/                   ← Orquestrador Aspire
    │   └── AppHost.cs                       ← Server + Web frontend
    │
    └── frontend/                            ← React Web Dashboard
        ├── package.json                     ← React 18, Vite 7, shadcn/ui, etc.
        ├── vite.config.ts                   ← Proxy /api → localhost:5481
        ├── tailwind.config.js
        ├── tsconfig.json
        └── src/
            ├── main.tsx / App.tsx           ← Entry + Router com auth guards
            ├── contexts/                    ← AuthContext + AppContext
            ├── services/api.ts              ← Axios + JWT interceptor
            ├── models/index.ts              ← TypeScript interfaces
            ├── pages/                       ← 13 páginas
            ├── components/ui/               ← shadcn/ui (47 componentes)
            ├── components/common/           ← LoadingSpinner, Pagination
            ├── layout/Layout.tsx
            ├── lib/                         ← query client, cn()
            └── styles/globals.css           ← Tema laranja + utility classes
```

## Stack

| Camada | Tecnologia |
|--------|-----------|
| Mobile | .NET MAUI (.NET 10), C#, XAML, MVVM |
| Web | React 18, TypeScript 5.9, Vite 7, Tailwind CSS 3, shadcn/ui |
| API | ASP.NET Core (.NET 10), Entity Framework Core 10 |
| DB | PostgreSQL 16 (Npgsql) |
| Auth | JWT Bearer + BCrypt |
| Orquestração | .NET Aspire (service discovery, health checks, OpenTelemetry) |
| Libs Web | TanStack React Query, Axios, React Router v7, Zod, Recharts, date-fns, sonner |

## Modelo de Dados

### Entidades

| Entidade | Descrição |
|----------|-----------|
| **Usuario** | Usuários (Admin/Operator), senha hasheada com BCrypt |
| **Produto** | Produtos MP (matéria-prima) e PA (acabado), com controle de estoque |
| **OrdemProducao** | Ordens de produção (PLANEJADA / EM_PRODUCAO / PAUSADA / CONCLUIDA / CANCELADA) |
| **Movimentacao** | Movimentações (ENTRADA / SAIDA / PRODUCAO / CONSUMO) |
| **NotaFiscal** / **NotaFiscalItem** | Notas fiscais de entrada (MP) e saída (PA) |
| **Materiais** / **MateriaisItems** | Ficha técnica (BOM / receita) |
| **Estoque** | Snapshot de estoque |
| **Vendas** | Registro de vendas |

### Enumeradores

| Enum | Valores |
|------|---------|
| `UserRole` | Admin = 1, Operator = 2 |
| `TipoProduto` | MP, PA |
| `TipoMovimentacao` | ENTRADA, SAIDA, PRODUCAO, CONSUMO |
| `OrdemProducaoStatus` | PLANEJADA, EM_PRODUCAO, PAUSADA, CONCLUIDA, CANCELADA |
| `NotaFiscalTipo` | ENTRADA, SAIDA |
| `NotaFiscalStatus` | PROCESSADA, PENDENTE, ERRO |

## API — Endpoints

### Usuario
| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/usuario/register` | Criar conta (operator) |
| POST | `/api/usuario/login` | Login → JWT |
| GET | `/api/usuario` | Listar usuários |
| PUT | `/api/usuario/{id}` | Atualizar usuário |
| DELETE | `/api/usuario/{id}` | Deletar usuário |

### Produto
| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/produto` | Criar produto (auth) |
| GET | `/api/produto` | Listar (inclui BOM) |
| GET | `/api/produto/{id}` | Buscar por ID |
| PUT | `/api/produto/{id}` | Atualizar produto |
| DELETE | `/api/produto/{id}` | Deletar produto |

### ProductionOrder
| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/ProductionOrder` | Criar ordem (calcula BOM) |
| GET | `/api/ProductionOrder` | Listar (inclui produto + usuário) |
| PUT | `/api/ProductionOrder/{id}` | Atualizar status + quantidade produzida |

### BOM (Ficha Técnica)
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/BOM` | Listar todas as fichas técnicas |
| GET | `/api/BOM/{id}` | Buscar BOM por ID |
| POST | `/api/BOM` | Criar BOM com materiais |

### Movimentacao
| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/movimentacao` | Criar movimentação (entrada/saída/produção/consumo) |
| GET | `/api/movimentacao` | Listar (filtros: search, productId, type, startDate, endDate) |

### NotaFiscal
| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/notafiscal` | Criar (valida MP/PA, atualiza estoque em transação) |
| GET | `/api/notafiscal` | Listar (inclui itens) |

### Relatorio
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/relatorio/producao` | Produção por período |
| GET | `/api/relatorio/estoque` | Snapshot de estoque |
| GET | `/api/relatorio/consumo-mp` | Consumo de MP por período |
| GET | `/api/relatorio/vendas` | Vendas por período |

## Setup

### Pré-requisitos
- .NET 10 SDK
- Node.js ≥ 18.x + npm
- PostgreSQL 16 (porta 5432)
- Workloads: `dotnet workload install maui aspire`
- EF CLI: `dotnet tool install --global dotnet-ef`

### Instalação

```bash
# 1. Banco de dados
psql -U postgres -c "CREATE DATABASE \"SmartProd\";"

# 2. Migrations
cd SmartProd/SmartProd.Server
dotnet ef migrations add InitialCreate
dotnet ef database update

# 3. Frontend
cd SmartProd/frontend
npm install
```

### Rodar

```bash
# Terminal 1 — API (http://localhost:5481)
cd SmartProd/SmartProd.Server
dotnet run

# Terminal 2 — Web (http://localhost:5173)
cd SmartProd/frontend
npm run dev

# Terminal 3 — Mobile Android
cd SmartProd_Mobile_Front-end
dotnet build -t:Run -f net10.0-android

# Terminal 4 — Aspire (opcional)
cd SmartProd/SmartProd.AppHost
dotnet run
```

### Primeiro Acesso

```bash
# Criar admin
curl -X POST http://localhost:5481/api/usuario/register \
  -H "Content-Type: application/json" \
  -d '{"name":"Admin","email":"admin@smartprod.com","senha":"123456"}'

# Login no web com admin@smartprod.com / 123456
```

## Correções Aplicadas ao Código

| # | Correção | Arquivo |
|---|----------|---------|
| 1 | Removido `AuthController.cs` (stub duplicado conflitava rota de login) | `Middlewares/AuthController.cs` |
| 2 | Renomeado `Serveces` → `Services` | Pasta + 6 services |
| 3 | Renomeado `ProducaoOrdemService.cs` → `ProductionOrderService.cs` | Services |
| 4 | Renomeado `ProducaoOrdemController.cs` → `ProductionOrderController.cs` | Controllers |
| 5 | Removida exclusão `Services\**` do `.csproj` (impedia compilação) | `SmartProd.Server.csproj` |
| 6 | `DisplayAlertAsync` → `DisplayAlert` (método correto MAUI) | 3 ViewModels |
| 7 | Adicionado `android:usesCleartextTraffic="true"` (permitir HTTP no Android) | `AndroidManifest.xml` |
| 8 | `darkMode: ["darks"]` → `["class"]` | `tailwind.config.js` |
| 9 | `@types/react ^19.2.6` → `^18.2.0` (compatível com React 18) | `package.json` |
| 10 | Adicionado `stockMovements` e `addStockMovement` ao `AppContextType` | `AppContext.tsx` |
| 11 | Criado `RegistrationViewModel` com validação e comando de registro | `ViewsModels/RegistrationViewModel.cs` |
| 12 | Adicionado `RegisterAsync` e `CreateMovimentacaoAsync` no `ApiService` | `Services/ApiService.cs` |
| 13 | Conectado `RegistrationPage` ao ViewModel (bindings + DI) | `Views/registrationpage.xaml` + `.xaml.cs` |
| 14 | Corrigido `AdminDashboardPage_ViewsModel` para `public` + DI do `ApiService` + carregamento de dados reais da API | `ViewsModels/AdminDashboardPage_ViewsModel.cs` |
| 15 | Corrigido `ScannerPage` com DI + `ScannerViewModel` com chamada real à API | `Views/scannerpage.xaml.cs` + `ViewsModels/Scanner_ViewModel.cs` |
| 16 | `Loginpage_ViewModel` com role detection via API (não mais por email) | `ViewsModels/Loginpage_ViewModel.cs` |
| 17 | Adicionado campo `Role` no `UsuarioResponseDto` (backend) e `UsuarioResponse` (mobile) | `DTOs/UsuarioResponseDto.cs` + `Models/UsuarioResponse.cs` |
| 18 | Criado **BOMController** (GET/POST `/api/BOM`) | `Controllers/BOMController.cs` |
| 19 | Adicionado PUT/DELETE Produto + Service | `Controllers/ProdutoController.cs` + `Services/ProdutoService.cs` |
| 20 | Adicionado PUT ProductionOrder (status + produced) + Service | `Controllers/ProductionOrderController.cs` + `Services/ProductionOrderService.cs` |
| 21 | Adicionado POST Movimentacao + Service | `Controllers/MovimentacaoController.cs` + `Services/MovimentoEstoqueService.cs` |
| 22 | Adicionado PUT/DELETE Usuario + Service | `Controllers/UsuarioController.cs` + `Services/UsuarioService.cs` |
| 23 | DTOs: `UpdateOrderStatusDto`, `CreateMovimentacaoDto`, `UpdateUserDto` | `DTOs/` |
| 24 | **AppContext** web sem fallbacks locais — dados vêm todos da API | `frontend/src/contexts/AppContext.tsx` |
| 25 | Adicionado BOM fetching no AppContext + refreshAll | `frontend/src/contexts/AppContext.tsx` |
| 26 | **AuthContext** com mapeamento de role (`Admin/Operator` → `admin/operador`) | `frontend/src/contexts/AuthContext.tsx` |
| 27 | Vite proxy sem rewrite incorreta | `frontend/vite.config.ts` |
| 28 | **Dashboard mobile com dados reais da API** — removidos cards estáticos (42 operadores, 94,8% eficiência, 03 alertas, 1.240 un/h); agora bindings para API | `ViewsModels/AdminDashboardPage_ViewsModel.cs` + `Views/AdminDashboardPage.xaml` |
| 29 | Criado model `MovimentacaoResponse` para deserializar resposta de GET `/api/movimentacao` | `Models/MovimentacaoResponse.cs` |
| 30 | Adicionado `GetStockMovementsAsync()` ao `ApiService` | `Services/ApiService.cs` |

## Status Atual do Projeto (28/05/2026)

| Componente | Status | Detalhes |
|------------|--------|----------|
| **API** | ✅ Rodando | `http://localhost:5481` |
| **Frontend Web** | ✅ Rodando | `http://localhost:5173` |
| **Mobile MAUI** | ✅ Rodando | Conexão com backend via `http://localhost:5481` |
| **Banco PostgreSQL** | ✅ Conectado | `SmartProd` database com migrations aplicadas |
| **Usuário Admin** | ✅ Criado | `admin@email.com` / `123456` (role: Admin, token JWT funcional) |
| **Bug DateTime** | ✅ Corrigido | `DateTime.Now` → `DateTime.UtcNow` em 8 arquivos (modelos e services) |

### Conexão Mobile ↔ Web ↔ API

**Mobile, Web Dashboard e API compartilham o mesmo backend** (`http://localhost:5481`) e o mesmo banco PostgreSQL.  
Dados cadastrados no mobile (ex: login, ordens de produção) aparecem no web dashboard e vice-versa.

| Conexão | Status |
|---------|--------|
| Mobile → API | ✅ `ApiService` → `http://localhost:5481` |
| Web → API | ✅ Axios → `http://localhost:5481` (sem fallback local) |
| API → Banco | ✅ PostgreSQL via EF Core |
| Mobile → Web | ✅ Indireta: ambos usam mesma API/BD |

## Redesign das Telas MAUI (28/05/2026)

Todas as 5 páginas do aplicativo mobile foram redesenhadas com a identidade visual SmartProd, mantendo 100% das funcionalidades e bindings originais.

### Paleta de Cores

| Cor | Uso | Hex |
|-----|-----|-----|
| **Laranja SmartProd** | Primary, botões, acentos | `#FF8C00` |
| **Laranja Escuro** | Hover, variante | `#E67A00` |
| **Dark Navy** | Fundos escuros, headings | `#071E27` |
| **Teal** | Status positivo, sucesso | `#059669` |
| **Cinza Claro** | Background da página | `#F8F6F2` |
| **Superfície** | Cards, containers | `#FFFFFF` |

### Estilos Globais (`Styles.xaml`)

| Componente | Estilo |
|------------|--------|
| **Card** (`Border`) | RoundRectangle 12px, sombra suave, padding 20px |
| **Botão Primário** | Laranja (`#FF8C00`), bold, corner 10px, height 50px |
| **Botão Secundário** | Background laranja claro, texto laranja |
| **Input** | Background `#F3F4F6`, corner 10px, padding 16x12 |
| **Cabeçalhos** | `Title`: 28px bold, `Subtitle`: 15px, `Caption`: 11px uppercase |

### Telas Redesenhadas

| Página | Destaques |
|--------|-----------|
| **LoginPage** | Card central com sombra, inputs arredondados, botão laranja full-width |
| **RegistrationPage** | Mesmo estilo do login, layout limpo e moderno |
| **ProductionPage** | Header branco, card de eficiência em dark navy, cards de ordem com sombra e status badges |
| **ScannerPage** | Fundo escuro (`#071E27`), frame de scan com cantos laranja, cards translúcidos |
| **AdminDashboardPage** | KPIs com dados reais da API: produtos (total + MP/PA), ordens de produção (total + ativas + total produzido), movimentações (total + última + lista recente), alertas de estoque baixo; botão de atualizar; loading indicator |

### Arquivos Modificados

| Arquivo | Descrição |
|---------|-----------|
| `Resources/Styles/Colors.xaml` | Paleta completa SmartProd |
| `Resources/Styles/Styles.xaml` | Estilos globais (cards, botões, inputs, tipografia) |
| `AppShell.xaml` | Navegação por abas com fundo laranja |
| `Views/LoginPage.xaml` | Redesign completo |
| `Views/RegistrationPage.xaml` | Redesign completo |
| `Views/ProductionPage.xaml` | Redesign completo |
| `Views/ScannerPage.xaml` | Redesign completo |
| `Views/AdminDashboardPage.xaml` | Redesign completo → dados reais da API (bindings removidos valores estáticos) |
| `ViewsModels/AdminDashboardPage_ViewsModel.cs` | Propriedades reais: produtos, ordens, movimentações, alertas estoque |
| `Models/MovimentacaoResponse.cs` | Modelo para resposta GET /api/movimentacao |
