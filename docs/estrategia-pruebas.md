# Estrategia de pruebas

## Enfoque

- `StockFlow.UnitTests`: reglas pequenas y calculos puros del dominio/aplicacion.
- `StockFlow.IntegrationTests`: flujo HTTP real con autenticacion, multi-tenant y persistencia EF Core sobre SQL Server real ejecutado con Testcontainers.
- La suite aplica migraciones reales de EF Core al iniciar y resetea datos entre tests con Respawn, sin recurrir a `EnsureCreated()` ni a SQLite en memoria.

## Casos cubiertos actualmente

- calculo de campos derivados de producto
- validacion de rango de fechas
- rechazo de endpoints privados sin token
- registro y consulta del usuario autenticado sin exponer `PasswordHash`
- flujo de venta con descuento de stock e historial de inventario
- rechazo de venta con stock insuficiente sin persistir venta ni movimientos
- concurrencia de ventas sobre el mismo producto sin sobreventa
- aislamiento por `BusinessId` entre negocios

## Comandos

```bash
dotnet test StockFlow.sln
```

Requisito para integracion:

- Docker disponible y encendido en la maquina o agente CI.

## Gaps conocidos

- faltan pruebas dedicadas para duplicidad de `InternalCode`
- faltan pruebas especificas para inventario manual `Exit` y `Adjustment`
- faltan pruebas para todos los reportes con datos variados
