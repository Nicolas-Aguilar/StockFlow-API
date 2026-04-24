# StockFlow API

StockFlow API es una API REST en ASP.NET Core Web API para gestionar negocio, categorias, productos, inventario, ventas y reportes operativos de un pequeno negocio, respetando aislamiento por `BusinessId`, DTOs publicos y reglas de negocio criticas.

> Estado actual: base funcional de la version 1 implementada con autenticacion JWT, EF Core, SQL Server, Swagger, pruebas y documentacion tecnica.

## Stack

- C# / .NET 8
- ASP.NET Core Web API
- Entity Framework Core + SQL Server
- JWT Bearer Authentication
- Swagger / OpenAPI
- xUnit
- Docker Compose para SQL Server local

## Arquitectura

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

## Reglas clave aplicadas

- Todo dato operativo usa `BusinessId`.
- Ninguna consulta operativa busca solo por `Id`; tambien valida `BusinessId`.
- La API responde con DTOs; no expone entidades directamente.
- Las ventas calculan total y ganancia en backend.
- Cada venta descuenta stock y crea `InventoryMovement` tipo `Sale` dentro de una transaccion.
- Un producto con historial no se elimina fisicamente: se desactiva.
- Un producto inactivo o caducado no puede venderse.

## Configuracion local

### 1. Levantar SQL Server

```bash
docker compose up -d
```

Cadena de conexion local por defecto:

```text
Server=localhost,1433;Database=StockFlowDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;
```

### 2. Restaurar y compilar

```bash
dotnet restore
dotnet build StockFlow.sln
```

### 3. Aplicar migraciones

El repositorio incluye migracion inicial y manifiesto de herramienta local para EF Core.

```bash
dotnet tool restore
dotnet ef database update --project src/StockFlow.Infrastructure --startup-project src/StockFlow.Api
```

### 4. Ejecutar la API

```bash
dotnet run --project src/StockFlow.Api
```

Swagger queda disponible en `/swagger`.

## Verificacion usada en esta base

```bash
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

## Pendientes razonables

- endurecer validaciones con una capa dedicada de validadores si el dominio crece
- agregar seeds opcionales para demo
- ampliar cobertura automatizada para mas escenarios de reportes e inventario
- documentar ejemplos HTTP completos por endpoint
