# Reglas de negocio

## Multi-tenant por negocio

- Todo dato operativo pertenece a un `BusinessId`.
- Categorias, productos, movimientos, ventas y reportes se filtran por el negocio autenticado.
- Ninguna operacion operativa consulta solo por `Id`.

## Productos

- `Name` e `InternalCode` son obligatorios.
- `InternalCode` es unico dentro del negocio.
- `PurchasePrice` no puede ser negativo.
- `SalePrice` debe ser mayor que cero y mayor o igual a `PurchasePrice`.
- `CurrentStock` y `MinimumStock` no pueden ser negativos.
- Un producto inactivo o caducado no puede venderse.
- Si tiene historial de ventas o movimientos, `DELETE` lo desactiva en lugar de eliminarlo.

## Inventario

- Todo cambio de stock deja historial en `InventoryMovements`.
- `Entry` suma stock.
- `Exit` y `Adjustment` descuentan stock y requieren motivo.
- `Sale` solo se genera automaticamente desde ventas.
- No se permite dejar stock negativo.

## Ventas

- Una venta debe incluir al menos un item.
- Cada item debe tener cantidad mayor que cero.
- El backend calcula `Subtotal`, `Total` y `EstimatedProfit`.
- Si un producto falla por negocio, estado, caducidad o stock, toda la venta falla.
- La venta crea `Sale`, `SaleItems`, descuenta stock y registra `InventoryMovement` tipo `Sale` en una misma transaccion.

## Reportes

- Son solo lectura.
- Filtran siempre por `BusinessId`.
- Resumenes por fecha validan `from <= to`.
- Solo se exponen los reportes definidos para version 1.
