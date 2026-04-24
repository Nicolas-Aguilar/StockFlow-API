# Modelo de base de datos

## Entidades principales

### Users

- `Id`
- `FullName`
- `Email` (unico)
- `PasswordHash`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`

### Businesses

- `Id`
- `OwnerUserId`
- `Name`
- `Description`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`

Relacion version 1: `User 1 -> 1 Business`.

### Categories

- `Id`
- `BusinessId`
- `Name`
- `Description`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`

Indice unico: `BusinessId + Name`.

### Products

- `Id`
- `BusinessId`
- `CategoryId`
- `Name`
- `InternalCode`
- `Description`
- `PurchasePrice`
- `SalePrice`
- `CurrentStock`
- `MinimumStock`
- `ExpirationDate`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`

Indice unico: `BusinessId + InternalCode`.

Campos calculados en API, no persistidos:

- `ProfitPerUnit`
- `ProfitMarginPercentage`
- `IsLowStock`
- `IsExpired`
- `DaysUntilExpiration`

### InventoryMovements

- `Id`
- `BusinessId`
- `ProductId`
- `MovementType`
- `Quantity`
- `Reason`
- `CreatedAt`

### Sales

- `Id`
- `BusinessId`
- `Total`
- `EstimatedProfit`
- `PaymentMethod`
- `CreatedAt`

### SaleItems

- `Id`
- `SaleId`
- `ProductId`
- `Quantity`
- `UnitPrice`
- `UnitPurchasePrice`
- `Subtotal`
- `EstimatedProfit`

## Relaciones clave

- `Businesses.OwnerUserId -> Users.Id`
- `Categories.BusinessId -> Businesses.Id`
- `Products.BusinessId -> Businesses.Id`
- `Products.CategoryId -> Categories.Id`
- `InventoryMovements.BusinessId -> Businesses.Id`
- `InventoryMovements.ProductId -> Products.Id`
- `Sales.BusinessId -> Businesses.Id`
- `SaleItems.SaleId -> Sales.Id`
- `SaleItems.ProductId -> Products.Id`

## Migraciones

La migracion inicial esta en `src/StockFlow.Infrastructure/Migrations` y fue generada con EF Core 8.
