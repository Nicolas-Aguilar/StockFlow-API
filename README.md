# StockFlow API

StockFlow API es una API REST en ASP.NET Core Web API orientada a un caso de negocio real: ayudar a un pequeno comercio a controlar catalogo, inventario, ventas, caducidad y reportes operativos sin sacrificar seguridad, reglas de negocio ni mantenibilidad.

Es un proyecto pensado tanto para uso local y aprendizaje practico como para portafolio backend: arquitectura por capas, autenticacion JWT, EF Core con SQL Server, Swagger, pruebas automatizadas y documentacion tecnica consistente.

> Estado actual: base funcional de la version 1 implementada con autenticacion JWT, EF Core, SQL Server, Swagger, pruebas y documentacion tecnica.

## Quickstart

### Opcion recomendada: bootstrap automatico

PowerShell:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\bootstrap.ps1
```

PowerShell + API levantada al final:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\bootstrap.ps1 -RunApi
```

Bash:

```bash
./scripts/bootstrap.sh
```

Bash + API levantada al final:

```bash
./scripts/bootstrap.sh --run-api
```

El bootstrap hace de forma idempotente:

- crea `.env` local si no existe
- genera credenciales locales para SQL Server y JWT fuera del repositorio
- sincroniza `dotnet user-secrets` en `src/StockFlow.Api`
- levanta SQL Server con Docker Compose y espera el healthcheck
- ejecuta `dotnet tool restore`, `dotnet restore`, `dotnet ef database update` y `dotnet build`

## Que aporta este backend

- Gestiona autenticacion, negocio, categorias, productos, inventario, ventas y reportes operativos desde una misma API
- Aplica aislamiento por `BusinessId` para evitar cruces entre negocios
- Mantiene controladores delgados y reglas de negocio en capas de aplicacion y dominio
- Calcula total, ganancia y movimientos de inventario en backend, no en el cliente
- Usa SQL Server real en desarrollo local y pruebas de integracion con flujo HTTP completo

## Stack y arquitectura

### Stack principal

- C# / .NET 8
- ASP.NET Core Web API
- Entity Framework Core + SQL Server
- JWT Bearer Authentication
- Swagger / OpenAPI
- xUnit
- Docker Compose para SQL Server local

### Arquitectura

La solucion sigue arquitectura por capas:

```text
StockFlow.Api -> StockFlow.Application -> StockFlow.Domain
StockFlow.Infrastructure -> StockFlow.Application + StockFlow.Domain
```

Proyectos incluidos:

- `src/StockFlow.Api`
- `src/StockFlow.Application`
- `src/StockFlow.Domain`
- `src/StockFlow.Infrastructure`
- `tests/StockFlow.UnitTests`
- `tests/StockFlow.IntegrationTests`

## Modulos implementados

- Auth: `register`, `login`, `me`
- Business: `GET/PUT /api/businesses/me`
- Categories: crear, listar, consultar, actualizar y desactivar
- Products: crear, listar, consultar, buscar, stock bajo, proximos a caducar, caducados, actualizar, desactivar y eliminar segun historial
- Inventory: movimientos manuales, listado e historial por producto
- Sales: creacion transaccional, consulta general, por id y por rango de fechas
- Reports: bajo stock, proximos a caducar, caducados, mas vendidos, resumen de ventas, resumen de ganancias y valoracion de inventario

## Reglas backend destacadas

- Todo dato operativo usa `BusinessId`
- Ninguna consulta operativa busca solo por `Id`; tambien valida `BusinessId`
- La API responde con DTOs; no expone entidades directamente
- Las ventas calculan total y ganancia en backend
- Cada venta descuenta stock y crea `InventoryMovement` tipo `Sale` dentro de una transaccion
- Un producto con historial no se elimina fisicamente: se desactiva
- Un producto inactivo o caducado no puede venderse

## Configuracion local segura

StockFlow no versiona credenciales operativas. Para desarrollo local se usa esta estrategia:

- Docker Compose toma la password de SQL Server desde un archivo local `.env` ignorado por git
- La API y EF Core toman `ConnectionStrings:DefaultConnection` y `Jwt:Key` desde `dotnet user-secrets` o variables de entorno
- `appsettings.json` solo conserva placeholders y valores no sensibles

> Importante: si alguna credencial local quedo expuesta en el historial antes de este enfoque, considerala comprometida y no la reutilices fuera de tu maquina.

## Comandos importantes

### Bootstrap / setup

Opcion recomendada en Windows:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\bootstrap.ps1
```

Alternativa en Bash:

```bash
./scripts/bootstrap.sh
```

### Ejecutar la API

```bash
dotnet run --project src/StockFlow.Api
```

Swagger queda disponible en `/swagger`.

### Compilar

```bash
dotnet build StockFlow.sln
```

### Probar

```bash
dotnet test StockFlow.sln
```

### Caveat de pruebas de integracion

Las pruebas de `tests/StockFlow.IntegrationTests` usan Testcontainers, SQL Server real y migraciones de EF Core; requieren Docker disponible y encendido.

Si quieres ejecutar solo integracion:

```bash
dotnet test tests/StockFlow.IntegrationTests/StockFlow.IntegrationTests.csproj
```

## Flujo manual equivalente

### 1. Preparar variables locales para Docker

```bash
cp .env.example .env
```

Edita `.env` y define al menos una password local para `SQLSERVER_SA_PASSWORD`.

Cadena de conexion esperada para user-secrets o variables de entorno:

```text
Server=localhost,1433;Database=StockFlowDb;User Id=sa;Password=<tu-password-local>;TrustServerCertificate=True;
```

### 2. Levantar SQL Server local

```bash
docker compose up -d
```

### 3. Configurar secretos de la API

Opcion recomendada para desarrollo local:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=StockFlowDb;User Id=sa;Password=<tu-password-local>;TrustServerCertificate=True;" --project src/StockFlow.Api
dotnet user-secrets set "Jwt:Key" "<tu-clave-jwt-local-larga-y-unica>" --project src/StockFlow.Api
```

Alternativa para CI, shells efimeros o contenedores:

```bash
ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=StockFlowDb;User Id=sa;Password=<tu-password-local>;TrustServerCertificate=True;"
Jwt__Key="<tu-clave-jwt-local-larga-y-unica>"
```

`Jwt:Issuer`, `Jwt:Audience` y `Jwt:ExpirationMinutes` permanecen en `appsettings.json` porque no son secretos.

### 4. Restaurar y compilar

```bash
dotnet restore
dotnet build StockFlow.sln
```

### 5. Aplicar migraciones

```bash
dotnet tool restore
dotnet ef database update --project src/StockFlow.Infrastructure --startup-project src/StockFlow.Api
```

`AppDbContextFactory` usa la misma configuracion que la API: `appsettings.json`, `appsettings.{Environment}.json`, user-secrets y variables de entorno. Si falta `ConnectionStrings:DefaultConnection`, `dotnet ef` falla con un mensaje explicito.

### 6. Ejecutar la API

```bash
dotnet run --project src/StockFlow.Api
```

Si intentas iniciar la API sin `ConnectionStrings:DefaultConnection` o `Jwt:Key`, el arranque falla de forma intencional para evitar configuraciones inseguras o ambiguas.

## Verificacion base

```bash
dotnet restore
dotnet build StockFlow.sln
dotnet test StockFlow.sln
```

## Documentacion tecnica

- `docs/reglas-negocio.md`
- `docs/modelo-base-datos.md`
- `docs/endpoints-api.md`
- `docs/decisiones-tecnicas.md`
- `docs/roadmap-desarrollo.md`
- `docs/seguridad.md`
- `docs/estrategia-pruebas.md`

## Proximos pasos razonables

- ampliar cobertura automatizada en reportes e inventario manual
- endurecer validaciones de entrada con una capa dedicada si el dominio sigue creciendo
- agregar ejemplos HTTP completos y capturas de Swagger para reforzar el enfoque de portafolio
- incorporar seeds opcionales de demo para acelerar revisiones funcionales
