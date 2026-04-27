# Decisiones tecnicas

## 1. Arquitectura por capas

- Se mantiene separacion estricta entre `Api`, `Application`, `Domain` e `Infrastructure`.
- Los controladores delegan toda la logica a servicios de aplicacion.

## 2. Aislamiento por `BusinessId`

- Se implementa `IUserContext` basado en claims JWT.
- Los servicios aplican `BusinessId` en todas las consultas operativas.

## 3. EF Core con repositorios especificos

- Se usan repositorios concretos por agregado para mantener consultas expresivas del negocio.
- No se agrego un repositorio generico ni un `UnitOfWork` artificial.

## 4. JWT sencillo y seguro para version 1

- Se usa `Microsoft.AspNetCore.Authentication.JwtBearer`.
- El token incluye `userId`, `businessId` y `email`, sin datos sensibles.

## 5. Hash de contrasenas

- Se usa `PasswordHasher<User>` de ASP.NET Core.
- No se persisten contrasenas en texto plano.

## 6. Transaccion para ventas

- `SaleRepository.ExecuteInTransactionAsync` encapsula la operacion transaccional obligatoria.
- Dentro de la transaccion se crean venta, items, movimientos y descuento de stock.
- El descuento de stock ahora usa un `UPDATE` atomico y condicional sobre `Products.CurrentStock` para evitar sobreventa y lost updates en concurrencia.
- Se agrego `CK_Products_CurrentStock_NonNegative` como defensa adicional a nivel de base de datos.

## 7. Herramientas agregadas y justificacion

- `Microsoft.EntityFrameworkCore.SqlServer`: persistencia obligatoria con SQL Server.
- `Microsoft.EntityFrameworkCore.Design`: migraciones y tooling EF Core.
- `Microsoft.Extensions.Identity.Core`: hashing seguro de contrasenas.
- `Microsoft.AspNetCore.Authentication.JwtBearer`: autenticacion JWT obligatoria.
- `Microsoft.AspNetCore.Mvc.Testing`: host HTTP real para integration tests.
- `Testcontainers.MsSql`: levanta SQL Server efimero y reproducible para integration tests locales y CI.
- `Respawn`: limpia datos entre tests sin recrear el esquema, manteniendo estabilidad y velocidad razonable sobre SQL Server.

## 8. Bootstrap local automatizado

- El onboarding local se automatiza con scripts idempotentes en `scripts/bootstrap.ps1` y `scripts/bootstrap.sh`.
- Los scripts generan `.env` local si no existe, crean una password de SQL Server solo para desarrollo y sincronizan `ConnectionStrings:DefaultConnection` y `Jwt:Key` en `dotnet user-secrets`.
- Despues levantan SQL Server con Docker Compose, esperan healthcheck, restauran herramientas/paquetes y aplican migraciones reales.
- El flujo preserva el arranque tradicional con `dotnet run`, pero reduce la configuracion manual a un comando reproducible.

## 9. Integration tests alineadas con produccion

- La suite deja de reemplazar SQL Server por SQLite y usa el provider real de produccion durante todo el ciclo HTTP + EF Core.
- El esquema se crea con `Database.Migrate()` para validar migraciones reales en cada corrida de integracion.
- La configuracion de prueba usa credenciales efimeras del contenedor y las expone al host de pruebas con alcance del proceso, evitando secretos reales y manteniendo precedencia sobre la configuracion local del desarrollador.
