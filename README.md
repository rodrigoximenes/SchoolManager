# SchoolManager

Backend de gestão escolar em .NET 9 com Clean Architecture, DDD, CQRS e multi-tenancy (database-per-tenant).

## Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [dotnet-ef CLI](https://learn.microsoft.com/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`

---

## Setup local (passo a passo)

### 1. Subir os containers

```bash
docker-compose up -d
docker ps  # postgres e mongo devem estar healthy
```

### 2. Configurar secrets locais

```bash
cd src/SchoolManager.WebApi

dotnet user-secrets init

dotnet user-secrets set "ConnectionStrings:Master" \
  "Host=localhost;Port=5432;Database=SchoolManager_Master;Username=schooladmin;Password=localpass123"

dotnet user-secrets set "MongoDb:ConnectionString" \
  "mongodb://mongoadmin:localpass123@localhost:27017"

dotnet user-secrets set "MongoDb:Database"    "SchoolManagerLogs"
dotnet user-secrets set "Jwt:SecretKey"       "chave-local-minimo-32-chars-xpto!!"
dotnet user-secrets set "Jwt:Issuer"          "schoolmanager-local"
dotnet user-secrets set "Jwt:Audience"        "schoolmanager-api"
```

### 3. Aplicar migration do banco Master

```bash
dotnet ef database update \
  --project src/SchoolManager.Infrastructure \
  --startup-project src/SchoolManager.WebApi \
  --context MasterDbContext
```

### 4. Rodar a API

```bash
cd src/SchoolManager.WebApi
dotnet run
```

API disponível em: `https://localhost:7000` | Swagger: `https://localhost:7000/swagger`

### 5. Criar a primeira escola (via Swagger)

```
POST /api/v1/admin/escolas
Authorization: Bearer {token-administrador}
{
  "nome": "Escola Teste",
  "cnpj": "00.000.000/0001-00"
}
```

Este endpoint cria o banco `SchoolManager_{EscolaId}`, aplica a migration de Escola e retorna as credenciais do Diretor.

---

## Criar nova migration

```bash
# Migration do banco Master
dotnet ef migrations add {NomeDaMigration} \
  --project src/SchoolManager.Infrastructure \
  --startup-project src/SchoolManager.WebApi \
  --context MasterDbContext \
  --output-dir Persistence/Migrations/Master

# Migration do banco de Escola
dotnet ef migrations add {NomeDaMigration} \
  --project src/SchoolManager.Infrastructure \
  --startup-project src/SchoolManager.WebApi \
  --context EscolaDbContext \
  --output-dir Persistence/Migrations/Escola
```

---

## Rodar testes

```bash
dotnet test
```

---

## Estrutura de pastas

```
SchoolManager.sln
├── src/
│   ├── SchoolManager.Domain/       → Entidades, VOs, eventos, regras de negócio
│   ├── SchoolManager.Application/  → Commands, Queries, Handlers, interfaces
│   ├── SchoolManager.Infrastructure/ → EF Core, repositórios, handlers de eventos
│   └── SchoolManager.WebApi/       → Controllers, middleware, Program.cs
└── tests/
    ├── SchoolManager.Domain.Tests/
    ├── SchoolManager.Application.Tests/
    ├── SchoolManager.Infrastructure.Tests/
    └── SchoolManager.WebApi.Tests/
```

---

## Convenção de branches

```
feat/criar-turma-PL-42
fix/refresh-token-expiracao-PL-87
test/handler-lancar-nota-PL-95
```

Formato de commit:
```
feat(turmas): adiciona CriarTurmaCommandHandler PL-42
fix(auth): corrige validacao de refresh token expirado PL-87
```

Fluxo: `feat/xxx` → PR → `develop` → PR → `main`

---

## Variáveis de ambiente em produção

Todas as secrets são providas via **Azure Key Vault** com ManagedIdentity.  
Nenhuma senha deve existir em arquivos de configuração no repositório.
