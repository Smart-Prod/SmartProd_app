# Configuração do SmartProd Server

Nunca versione valores reais de senhas, tokens JWT ou connection strings. `.env` é ignorado pelo Git; `.env.example` contém apenas valores de exemplo.

## Variáveis

| Variável | Uso | Obrigatória |
| --- | --- | --- |
| `DB_CONNECTION_STRING` | Connection string PostgreSQL. `SMARTPROD_DB_CONNECTION` e `ConnectionStrings__DefaultConnection` também são aceitas para compatibilidade. | Sim |
| `JWT_SECRET` | Chave de assinatura JWT com pelo menos 32 bytes. `SMARTPROD_JWT_SECRET` e `Jwt__Secret` são aliases legados. | Sim |
| `Jwt__Issuer` | Issuer do JWT. Padrão `SmartProd`. | Não |
| `Jwt__ExpiresInDays` | Validade do access token. Padrão 7 dias. | Não |
| `CORS_ORIGINS` | Origens web permitidas, separadas por vírgula. Padrão local: `http://localhost:5173`. | Não |
| `ADMIN_INITIAL_EMAIL` | E-mail do primeiro administrador; usado somente com banco sem usuários. | Sim para base vazia |
| `ADMIN_INITIAL_PASSWORD` | Senha do primeiro administrador, mínimo de 12 caracteres; nunca é registrada em log. | Sim para base vazia |
| `VITE_API_URL` | URL base do frontend; deixe vazia para API no mesmo domínio. | Não |

O backend aplica migrações ao iniciar. Um banco vazio exige as duas variáveis de administrador inicial; contas de usuário não são criadas com senha padrão ou pública. O administrador inicial não é recriado nem substituído em bases com usuários.

## Docker Compose

Na raiz do repositório, copie `.env.example` para `.env`, substitua todos os valores de exemplo e execute:

```powershell
docker compose up --build
```

O Compose lê `.env` para interpolação e injeta as variáveis necessárias nos containers. O PostgreSQL usa o volume nomeado `smartprod-postgres` e executa scripts da pasta `db-init` somente na inicialização de um volume novo.

## Execução local no PowerShell

ASP.NET Core não interpreta `.env` automaticamente. Defina variáveis no terminal ou use User Secrets para valores de desenvolvimento:

```powershell
Set-Location SmartProd\SmartProd.Server
dotnet user-secrets init
dotnet user-secrets set "DB_CONNECTION_STRING" "Host=localhost;Port=5432;Database=SmartProd;Username=postgres;Password=<sua-senha>"
dotnet user-secrets set "JWT_SECRET" "<segredo-aleatorio-com-pelo-menos-32-bytes>"
dotnet user-secrets set "ADMIN_INITIAL_EMAIL" "admin@localhost"
dotnet user-secrets set "ADMIN_INITIAL_PASSWORD" "<senha-inicial-com-pelo-menos-12-caracteres>"
$env:CORS_ORIGINS = "http://localhost:5173"
dotnet run
```

Para gerar um valor aleatório localmente:

```powershell
[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(64))
```

O frontend pode usar `VITE_API_URL` em seu ambiente de build. No desenvolvimento Vite, as chamadas `/api` são encaminhadas para a API local.
