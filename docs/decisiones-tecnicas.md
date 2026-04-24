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

## 7. Herramientas agregadas y justificacion

- `Microsoft.EntityFrameworkCore.SqlServer`: persistencia obligatoria con SQL Server.
- `Microsoft.EntityFrameworkCore.Design`: migraciones y tooling EF Core.
- `Microsoft.Extensions.Identity.Core`: hashing seguro de contrasenas.
- `Microsoft.AspNetCore.Authentication.JwtBearer`: autenticacion JWT obligatoria.
- `Microsoft.AspNetCore.Mvc.Testing` y `Microsoft.EntityFrameworkCore.Sqlite`: pruebas de integracion aisladas sin depender de SQL Server real.
