# AGENTS.md

## 1. Nombre del proyecto

**StockFlow API**

Repositorio sugerido:

```text
stockflow-api
```

---

## 2. Propósito del proyecto

StockFlow API es una API REST desarrollada con **ASP.NET Core Web API** para que el dueño de un pequeño negocio pueda gestionar productos, inventario, ventas, caducidad, márgenes de ganancia y reportes puntuales.

El proyecto debe construirse como una solución backend profesional, escalable y mantenible. No debe desarrollarse como un CRUD simple ni como una aplicación improvisada.

Este proyecto tiene dos objetivos principales:

1. Resolver un caso de negocio realista: control de productos, stock, ventas, ganancias y caducidad.
2. Servir como proyecto de portafolio profesional para demostrar buenas prácticas backend con .NET.

---

## 3. Alcance de la versión 1

La versión 1 debe enfocarse únicamente en las siguientes capacidades:

- Registro e inicio de sesión del dueño del negocio.
- Creación del negocio asociado al dueño.
- Gestión de categorías.
- Gestión de productos.
- Control de stock actual y stock mínimo.
- Fecha de caducidad por producto.
- Registro de movimientos de inventario.
- Registro de ventas.
- Descuento automático de stock al vender.
- Cálculo de ganancia por unidad.
- Cálculo de margen de ganancia.
- Cálculo de ganancia estimada por venta.
- Consulta de productos con bajo stock.
- Consulta de productos próximos a caducar.
- Consulta de productos caducados.
- Reportes puntuales de inventario, ventas y ganancias.
- Seguridad básica con JWT.
- Documentación técnica.
- Pruebas automatizadas para reglas críticas.

---

## 4. Funcionalidades fuera del alcance de la versión 1

No implementar en la versión 1:

- Apertura de caja.
- Cierre de caja.
- Control de caja diaria.
- Reportes de caja.
- Facturación legal.
- Integración tributaria.
- Pagos en línea.
- Pasarelas de pago.
- Múltiples empleados.
- Roles avanzados.
- Múltiples sucursales.
- Múltiples negocios por usuario.
- Gestión avanzada de proveedores.
- Compras a proveedores.
- Manejo de lotes por producto.
- Códigos de barras.
- Notificaciones por correo.
- Notificaciones por WhatsApp.
- Aplicación móvil.
- Frontend completo.
- Sistema POS completo.
- Inteligencia artificial.
- Microservicios.
- Event sourcing.
- Arquitectura multi-tenant avanzada.

Estas funcionalidades pueden considerarse mejoras futuras, pero no deben agregarse sin instrucción explícita.

---

## 5. Stack tecnológico obligatorio

Usar el siguiente stack:

```text
Lenguaje: C#
Framework: ASP.NET Core Web API
Base de datos: SQL Server
ORM: Entity Framework Core
Autenticación: JWT
Documentación API: Swagger / OpenAPI
Pruebas: xUnit
Contenedores: Docker y Docker Compose
Control de versiones: Git
```

No cambiar la tecnología principal sin autorización explícita.

No agregar librerías innecesarias. Si se agrega una librería nueva, debe existir una justificación clara en `docs/decisiones-tecnicas.md`.

---

## 6. Convenciones generales

### 6.1 Idioma del código

Todo el código debe estar en **inglés**.

Ejemplos correctos:

```text
Product
Category
Business
Sale
SaleItem
InventoryMovement
CreateProductRequest
ProductResponse
ProductService
```

Ejemplos incorrectos:

```text
Producto
Categoria
Negocio
Venta
MovimientoInventario
```

---

### 6.2 Idioma de la documentación

La documentación debe estar en **español**.

Ejemplos:

```text
README.md
AGENTS.md
docs/reglas-negocio.md
docs/modelo-base-datos.md
docs/endpoints-api.md
```

---

### 6.3 Convención de nombres en base de datos

Usar nombres en inglés y estilo **PascalCase**.

Tablas en plural:

```text
Users
Businesses
Categories
Products
InventoryMovements
Sales
SaleItems
```

Columnas en PascalCase:

```text
Id
BusinessId
ProductId
InternalCode
PurchasePrice
SalePrice
CurrentStock
MinimumStock
ExpirationDate
CreatedAt
UpdatedAt
```

Claves foráneas:

```text
UserId
OwnerUserId
BusinessId
CategoryId
ProductId
SaleId
```

---

### 6.4 Convención de endpoints

Los endpoints deben estar en inglés, en plural y con kebab-case cuando aplique.

Ejemplos correctos:

```http
/api/products
/api/products/low-stock
/api/products/expiring-soon
/api/reports/inventory-valuation
```

Ejemplos incorrectos:

```http
/api/producto
/api/productos/bajoStock
/api/reporte/valorInventario
```

---

## 7. Estructura obligatoria del repositorio

El repositorio debe seguir esta estructura base:

```text
stockflow-api/
│
├── README.md
├── AGENTS.md
├── .gitignore
├── docker-compose.yml
├── StockFlow.sln
│
├── src/
│   ├── StockFlow.Api/
│   │   ├── Controllers/
│   │   ├── Middlewares/
│   │   ├── Extensions/
│   │   ├── Filters/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
│   │
│   ├── StockFlow.Application/
│   │   ├── DTOs/
│   │   ├── Interfaces/
│   │   ├── Services/
│   │   ├── UseCases/
│   │   ├── Validators/
│   │   └── Common/
│   │
│   ├── StockFlow.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── Exceptions/
│   │   ├── ValueObjects/
│   │   └── Common/
│   │
│   └── StockFlow.Infrastructure/
│       ├── Data/
│       ├── Configurations/
│       ├── Repositories/
│       ├── Migrations/
│       └── Seed/
│
├── tests/
│   ├── StockFlow.UnitTests/
│   └── StockFlow.IntegrationTests/
│
└── docs/
    ├── reglas-negocio.md
    ├── modelo-base-datos.md
    ├── endpoints-api.md
    ├── decisiones-tecnicas.md
    ├── roadmap-desarrollo.md
    ├── seguridad.md
    └── estrategia-pruebas.md
```

No crear carpetas nuevas en la raíz sin una razón clara.

---

## 8. Arquitectura por capas

El proyecto debe usar arquitectura por capas.

Capas principales:

```text
StockFlow.Api
StockFlow.Application
StockFlow.Domain
StockFlow.Infrastructure
```

---

### 8.1 StockFlow.Api

Responsabilidad:

Exponer la API HTTP.

Puede contener:

- Controladores.
- Middlewares.
- Filtros.
- Configuración de Swagger.
- Configuración de autenticación.
- Configuración de autorización.
- Configuración de inyección de dependencias.
- Configuración general en `Program.cs`.

No debe contener:

- Lógica de negocio.
- Consultas directas con Entity Framework Core.
- Cálculos de ventas.
- Cálculos de ganancias.
- Cálculos de inventario.
- Validaciones complejas del dominio.

Los controladores deben ser delgados y delegar la lógica en servicios de aplicación.

---

### 8.2 StockFlow.Application

Responsabilidad:

Contener la lógica de aplicación y los casos de uso.

Puede contener:

- DTOs.
- Interfaces.
- Servicios.
- Casos de uso.
- Validadores.
- Contratos.
- Orquestación de procesos.
- Reglas de aplicación.

Ejemplos de lógica que debe vivir aquí:

- Crear producto.
- Actualizar producto.
- Registrar movimiento de inventario.
- Crear venta.
- Validar stock antes de vender.
- Calcular total de venta.
- Calcular ganancia de venta.
- Consultar productos con bajo stock.
- Consultar productos próximos a caducar.
- Generar reportes puntuales.

---

### 8.3 StockFlow.Domain

Responsabilidad:

Contener el núcleo del negocio.

Puede contener:

- Entidades.
- Enums.
- Excepciones de dominio.
- Objetos de valor.
- Reglas centrales del negocio.

Entidades principales:

```text
User
Business
Category
Product
InventoryMovement
Sale
SaleItem
```

Esta capa no debe depender de `Api`, `Application` ni `Infrastructure`.

---

### 8.4 StockFlow.Infrastructure

Responsabilidad:

Contener detalles técnicos e implementaciones concretas.

Puede contener:

- `AppDbContext`.
- Configuraciones de Entity Framework Core.
- Migraciones.
- Repositorios.
- Seed de datos.
- Implementaciones concretas de interfaces definidas en Application.

Esta capa puede depender de `Application` y `Domain`.

---

## 9. Reglas de dependencias

Respetar este flujo de dependencias:

```text
StockFlow.Api -> StockFlow.Application -> StockFlow.Domain
StockFlow.Infrastructure -> StockFlow.Application
StockFlow.Infrastructure -> StockFlow.Domain
```

Reglas obligatorias:

- `Domain` no debe depender de ninguna otra capa.
- `Application` puede depender de `Domain`.
- `Infrastructure` puede depender de `Application` y `Domain`.
- `Api` puede depender de `Application`.
- Los controladores no deben usar `DbContext` directamente.
- Los controladores no deben contener reglas de negocio.
- Las entidades no deben exponerse directamente como respuestas públicas de la API.
- Usar DTOs para entrada y salida de datos.

---

## 10. Modelo de base de datos

La base de datos debe estar diseñada para ser escalable desde la versión 1.

Aunque inicialmente el sistema será usado por un dueño de negocio, toda la información operativa debe estar asociada a un `BusinessId`.

Regla crítica:

```text
Todo dato operativo debe pertenecer a un negocio.
```

Esto permite crecer en el futuro hacia empleados, sucursales, permisos o múltiples negocios sin rediseñar toda la base.

---

### 10.1 Users

Representa al usuario dueño del negocio.

Tabla:

```text
Users
```

Columnas:

```text
Id
FullName
Email
PasswordHash
IsActive
CreatedAt
UpdatedAt
```

Reglas:

- `Email` es obligatorio.
- `Email` debe ser único.
- `PasswordHash` es obligatorio.
- No guardar contraseñas en texto plano.
- `IsActive` permite desactivar usuarios sin eliminarlos físicamente.

Índices:

```text
Unique: Email
```

---

### 10.2 Businesses

Representa el negocio del usuario.

Tabla:

```text
Businesses
```

Columnas:

```text
Id
OwnerUserId
Name
Description
IsActive
CreatedAt
UpdatedAt
```

Relaciones:

```text
User 1 -> 1 Business en versión 1
Business 1 -> N Categories
Business 1 -> N Products
Business 1 -> N InventoryMovements
Business 1 -> N Sales
```

Reglas:

- `OwnerUserId` es obligatorio.
- `Name` es obligatorio.
- Un negocio pertenece a un usuario dueño.
- En versión 1, un usuario tendrá un negocio principal.
- El diseño debe permitir ampliar a más negocios en el futuro si se decide.

---

### 10.3 Categories

Representa categorías de productos.

Tabla:

```text
Categories
```

Columnas:

```text
Id
BusinessId
Name
Description
IsActive
CreatedAt
UpdatedAt
```

Reglas:

- `BusinessId` es obligatorio.
- `Name` es obligatorio.
- Una categoría pertenece a un negocio.
- No eliminar físicamente una categoría si tiene productos asociados.
- Se debe usar desactivación cuando tenga historial asociado.

Índice recomendado:

```text
BusinessId + Name
```

---

### 10.4 Products

Representa productos del negocio.

Tabla:

```text
Products
```

Columnas:

```text
Id
BusinessId
CategoryId
Name
InternalCode
Description
PurchasePrice
SalePrice
CurrentStock
MinimumStock
ExpirationDate
IsActive
CreatedAt
UpdatedAt
```

Reglas:

- `BusinessId` es obligatorio.
- `CategoryId` es obligatorio.
- `Name` es obligatorio.
- `InternalCode` es obligatorio.
- `InternalCode` debe ser único dentro del mismo negocio.
- `PurchasePrice` no puede ser negativo.
- `SalePrice` debe ser mayor que cero.
- `SalePrice` debe ser mayor o igual que `PurchasePrice` en versión 1.
- `CurrentStock` no puede ser negativo.
- `MinimumStock` no puede ser negativo.
- `ExpirationDate` es opcional.
- Un producto inactivo no puede venderse.
- Un producto caducado no puede venderse.
- Un producto con ventas o movimientos no debe eliminarse físicamente; debe desactivarse.

Índice único recomendado:

```text
BusinessId + InternalCode
```

Campos calculados que no deben guardarse como columnas:

```text
ProfitPerUnit
ProfitMarginPercentage
IsLowStock
IsExpired
DaysUntilExpiration
```

Razón:

Estos valores dependen de otros datos y pueden calcularse al responder la API. Guardarlos podría causar inconsistencias.

---

### 10.5 InventoryMovements

Representa cualquier cambio de stock.

Tabla:

```text
InventoryMovements
```

Columnas:

```text
Id
BusinessId
ProductId
MovementType
Quantity
Reason
CreatedAt
```

Tipos de movimiento:

```text
Entry
Exit
Adjustment
Sale
```

Reglas:

- `BusinessId` es obligatorio.
- `ProductId` es obligatorio.
- `Quantity` debe ser mayor que cero.
- Todo cambio de stock debe generar un movimiento.
- Las salidas no pueden dejar stock negativo.
- Las ventas generan movimiento tipo `Sale`.
- `Reason` es obligatorio para movimientos manuales tipo `Exit` y `Adjustment`.
- Los movimientos de inventario no deben eliminarse físicamente.

---

### 10.6 Sales

Representa una venta.

Tabla:

```text
Sales
```

Columnas:

```text
Id
BusinessId
Total
EstimatedProfit
PaymentMethod
CreatedAt
```

Métodos de pago permitidos como dato informativo:

```text
Cash
Transfer
Card
Other
```

Reglas:

- `BusinessId` es obligatorio.
- Una venta pertenece a un negocio.
- `Total` se calcula en backend.
- `EstimatedProfit` se calcula en backend.
- No se debe confiar en totales enviados por el cliente.
- En versión 1, `PaymentMethod` no dispara lógica de caja.

---

### 10.7 SaleItems

Representa cada producto vendido dentro de una venta.

Tabla:

```text
SaleItems
```

Columnas:

```text
Id
SaleId
ProductId
Quantity
UnitPrice
UnitPurchasePrice
Subtotal
EstimatedProfit
```

Reglas:

- `SaleId` es obligatorio.
- `ProductId` es obligatorio.
- `Quantity` debe ser mayor que cero.
- `UnitPrice` se guarda al momento de la venta.
- `UnitPurchasePrice` se guarda al momento de la venta.
- `Subtotal` se calcula en backend.
- `EstimatedProfit` se calcula en backend.

Razón para guardar `UnitPrice` y `UnitPurchasePrice`:

Si el precio del producto cambia después, la venta histórica debe conservar los valores reales que tenía al momento de vender.

---

## 11. Reglas de negocio

Estas reglas son obligatorias y no deben romperse.

---

### 11.1 Reglas de usuario y negocio

- Cada usuario registrado debe tener un negocio asociado.
- Todo producto, categoría, venta y movimiento debe pertenecer a un `BusinessId`.
- Un usuario no puede acceder a datos de otro negocio.
- Las consultas deben filtrar por el `BusinessId` del usuario autenticado.
- No buscar entidades operativas solo por `Id`; siempre validar también `BusinessId`.

Ejemplo correcto:

```text
Buscar Product por Id y BusinessId.
```

Ejemplo incorrecto:

```text
Buscar Product solo por Id.
```

---

### 11.2 Reglas de productos

- El nombre del producto es obligatorio.
- El código interno es obligatorio.
- El código interno debe ser único dentro del negocio.
- El precio de compra no puede ser negativo.
- El precio de venta debe ser mayor que cero.
- El precio de venta debe ser mayor o igual que el precio de compra en versión 1.
- El stock actual no puede ser negativo.
- El stock mínimo no puede ser negativo.
- La fecha de caducidad es opcional.
- Si existe fecha de caducidad, debe ser válida.
- Un producto inactivo no puede venderse.
- Un producto caducado no puede venderse.
- Un producto sin historial puede eliminarse físicamente.
- Un producto con ventas o movimientos debe desactivarse, no eliminarse.

---

### 11.3 Reglas de inventario

- Todo cambio de stock debe generar un movimiento de inventario.
- La cantidad de un movimiento debe ser mayor que cero.
- Las salidas no pueden dejar stock negativo.
- Los ajustes deben tener motivo obligatorio.
- Las salidas manuales deben tener motivo obligatorio.
- Los movimientos de inventario no deben eliminarse físicamente.
- No se permite modificar stock manualmente sin historial.
- Una venta genera automáticamente movimiento de inventario tipo `Sale`.

---

### 11.4 Reglas de ventas

- Una venta debe tener al menos un producto.
- La cantidad de cada producto vendido debe ser mayor que cero.
- No se puede vender un producto inactivo.
- No se puede vender un producto caducado.
- No se puede vender más cantidad que el stock disponible.
- El total de la venta debe calcularse en backend.
- La ganancia estimada debe calcularse en backend.
- No se debe confiar en totales enviados por el cliente.
- Al crear una venta se debe descontar stock automáticamente.
- Al crear una venta se debe registrar un movimiento de inventario tipo `Sale`.
- Si un producto de la venta falla, toda la venta debe fallar.
- La creación de una venta debe manejarse como una operación transaccional.

---

### 11.5 Reglas de ganancias

Ganancia por unidad:

```text
ProfitPerUnit = SalePrice - PurchasePrice
```

Margen de ganancia:

```text
ProfitMarginPercentage = ((SalePrice - PurchasePrice) / SalePrice) * 100
```

Reglas:

- La ganancia por producto se calcula en backend.
- El margen de ganancia se calcula en backend.
- La ganancia de venta se calcula usando los precios guardados en `SaleItem`.
- Las ventas históricas no deben cambiar si luego se modifica el precio del producto.

---

### 11.6 Reglas de caducidad

- Un producto puede tener fecha de caducidad opcional.
- Un producto caducado no puede venderse.
- Producto caducado: `ExpirationDate` menor que la fecha actual.
- Producto próximo a caducar: `ExpirationDate` dentro del rango de días indicado.
- El rango por defecto para próximos a caducar será 30 días.
- El parámetro `days` debe ser mayor que cero.

---

### 11.7 Reglas de reportes

Los reportes permitidos en versión 1 son únicamente:

```text
LowStockProducts
ExpiringSoonProducts
ExpiredProducts
TopSellingProducts
SalesSummary
ProfitSummary
InventoryValuation
```

Los reportes deben responder preguntas puntuales:

- ¿Qué productos debo reponer?
- ¿Qué productos están próximos a caducar?
- ¿Qué productos ya caducaron?
- ¿Qué productos se venden más?
- ¿Cuánto vendí en un rango de fechas?
- ¿Cuánta ganancia estimada tuve en un rango de fechas?
- ¿Cuánto vale mi inventario actual?

Reglas:

- Los reportes son solo de lectura.
- Los reportes no deben modificar datos.
- Los reportes deben filtrar por `BusinessId`.
- Los reportes por fecha deben validar que `from` sea menor o igual a `to`.
- No crear reportes innecesarios sin instrucción explícita.

---

## 12. Patrones de diseño permitidos y uso esperado

No usar patrones por moda. Usar patrones solo cuando aporten claridad, escalabilidad o mantenibilidad.

---

### 12.1 Arquitectura por capas

Uso:

Todo el proyecto.

Razón:

- Separa responsabilidades.
- Evita lógica de negocio en controladores.
- Facilita pruebas.
- Permite escalar módulos sin desorden.

---

### 12.2 DTO Pattern

Uso:

Requests y responses de la API.

Ejemplos:

```text
CreateProductRequest
UpdateProductRequest
ProductResponse
CreateSaleRequest
SaleResponse
```

Razón:

- Evita exponer entidades directamente.
- Controla qué datos entran y salen.
- Protege la estructura interna.
- Facilita validaciones.

---

### 12.3 Service Pattern

Uso:

Lógica de aplicación.

Ejemplos:

```text
ProductService
CategoryService
InventoryService
SalesService
ReportService
AuthService
BusinessService
```

Razón:

- Mantiene controladores delgados.
- Centraliza reglas de aplicación.
- Permite pruebas de lógica sin depender directamente de HTTP.

---

### 12.4 Repository Pattern específico

Uso:

Repositorios específicos cuando aporten claridad.

Ejemplos:

```text
IProductRepository
ICategoryRepository
ISaleRepository
IInventoryMovementRepository
IBusinessRepository
```

No usar repositorio genérico tipo:

```text
IGenericRepository<T>
```

Razón para evitar repositorio genérico:

- Entity Framework Core ya actúa como Unit of Work y Repository.
- Un repositorio genérico puede ocultar consultas importantes del negocio.
- Puede agregar complejidad innecesaria.

Razón para permitir repositorios específicos:

- Permiten consultas expresivas del negocio.
- Mejoran pruebas.
- Evitan duplicar consultas complejas.

Ejemplos de métodos útiles:

```text
GetByInternalCodeAsync
GetLowStockProductsAsync
GetExpiringSoonProductsAsync
HasMovementsAsync
HasSalesAsync
GetTopSellingProductsAsync
```

---

### 12.5 Transacciones con Entity Framework Core

Uso obligatorio:

Creación de ventas.

Razón:

Crear una venta afecta varias partes:

```text
Sales
SaleItems
Products.CurrentStock
InventoryMovements
```

Si algo falla, toda la operación debe fallar.

Regla:

```text
La creación de una venta debe ser transaccional.
```

No crear una clase `UnitOfWork` genérica al inicio salvo que exista una necesidad real.

---

### 12.6 Domain Exceptions

Uso:

Errores de negocio controlados.

Ejemplos:

```text
InsufficientStockException
ProductExpiredException
ProductInactiveException
DuplicateInternalCodeException
BusinessAccessDeniedException
```

Razón:

- Permite errores claros.
- Evita mezclar errores técnicos con reglas de negocio.
- Facilita respuestas HTTP consistentes.

---

### 12.7 Query Object o Specification Pattern

Uso:

No obligatorio en versión 1.

Puede considerarse más adelante si los filtros crecen demasiado.

Ejemplos donde podría aplicar:

- Productos por categoría.
- Productos con bajo stock.
- Productos próximos a caducar.
- Ventas por rango de fechas.

En versión 1, preferir métodos específicos en servicios o repositorios antes de agregar más complejidad.

---

## 13. Seguridad

La seguridad debe implementarse desde el inicio.

---

### 13.1 Autenticación

Usar JWT.

Endpoints mínimos:

```http
POST /api/auth/register
POST /api/auth/login
GET /api/auth/me
```

Reglas:

- Los endpoints privados deben requerir token.
- El token debe incluir el `UserId`.
- El token no debe incluir información sensible.
- Las credenciales no deben aparecer en logs.
- No devolver `PasswordHash` en ninguna respuesta.

---

### 13.2 Contraseñas

Reglas:

- Nunca guardar contraseñas en texto plano.
- Guardar solo hash de contraseña.
- Usar `PasswordHasher` de ASP.NET Core o una estrategia segura equivalente.
- No devolver información sensible en errores de login.

Ejemplo de error aceptable:

```json
{
  "message": "Credenciales inválidas."
}
```

---

### 13.3 Autorización por negocio

Regla crítica:

```text
Un usuario no puede consultar, modificar ni eliminar datos de otro negocio.
```

Toda operación sobre datos operativos debe validar `BusinessId`.

Aplica a:

- Categories.
- Products.
- InventoryMovements.
- Sales.
- Reports.

Ejemplo correcto:

```text
GetProductAsync(productId, businessId)
```

Ejemplo incorrecto:

```text
GetProductAsync(productId)
```

---

### 13.4 Validación de entrada

Todo request debe validarse.

Validaciones mínimas:

- Campos obligatorios.
- Longitudes máximas.
- Valores monetarios válidos.
- Cantidades mayores que cero.
- Fechas válidas.
- Rango de fechas válido en reportes.
- Códigos internos no vacíos.
- Email con formato válido.

---

### 13.5 Protección de configuración

Reglas:

- No subir secretos reales al repositorio.
- No subir cadenas de conexión de producción.
- Usar variables de entorno para Docker.
- Incluir configuraciones de ejemplo cuando sea necesario.
- Mantener credenciales de desarrollo claramente marcadas como desarrollo local.

---

### 13.6 SQL Injection

Reglas:

- Usar Entity Framework Core y LINQ para consultas normales.
- No usar SQL crudo salvo que sea necesario.
- Si se usa SQL crudo, debe ser parametrizado.

---

### 13.7 Manejo de errores

Reglas:

- No exponer stack traces al cliente.
- No devolver detalles internos de excepciones.
- Usar middleware global de manejo de errores.
- Devolver mensajes claros y controlados.
- Usar códigos HTTP adecuados.

Ejemplos:

```json
{
  "message": "El producto no tiene stock suficiente."
}
```

```json
{
  "message": "El producto está caducado y no puede venderse."
}
```

```json
{
  "message": "El código interno del producto ya existe."
}
```

---

## 14. Reglas de API

La API debe seguir principios REST cuando sea posible.

Usar endpoints claros, consistentes y en inglés.

---

### 14.1 Auth

```http
POST /api/auth/register
POST /api/auth/login
GET /api/auth/me
```

---

### 14.2 Businesses

```http
GET /api/businesses/me
PUT /api/businesses/me
```

En versión 1, el negocio se crea al registrar el usuario o durante la configuración inicial del usuario.

---

### 14.3 Categories

```http
POST /api/categories
GET /api/categories
GET /api/categories/{id}
PUT /api/categories/{id}
PATCH /api/categories/{id}/deactivate
```

---

### 14.4 Products

```http
POST /api/products
GET /api/products
GET /api/products/{id}
GET /api/products/search?term=value
GET /api/products/low-stock
GET /api/products/expiring-soon?days=30
GET /api/products/expired
PUT /api/products/{id}
DELETE /api/products/{id}
PATCH /api/products/{id}/deactivate
```

Regla de eliminación:

```text
DELETE elimina físicamente solo si el producto no tiene ventas ni movimientos.
Si tiene historial, debe desactivarse.
```

---

### 14.5 Inventory

```http
POST /api/inventory/movements
GET /api/inventory/movements
GET /api/inventory/products/{productId}/history
```

---

### 14.6 Sales

```http
POST /api/sales
GET /api/sales
GET /api/sales/{id}
GET /api/sales/by-date?from=2026-01-01&to=2026-01-31
```

---

### 14.7 Reports

Solo se permiten estos reportes en versión 1:

```http
GET /api/reports/low-stock
GET /api/reports/expiring-soon?days=30
GET /api/reports/expired-products
GET /api/reports/top-selling-products
GET /api/reports/sales-summary?from=2026-01-01&to=2026-01-31
GET /api/reports/profit-summary?from=2026-01-01&to=2026-01-31
GET /api/reports/inventory-valuation
```

---

### 14.8 Códigos HTTP

Usar códigos HTTP adecuados:

```text
200 OK
201 Created
204 No Content
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
500 Internal Server Error
```

Ejemplos:

- `201 Created` al crear producto, categoría o venta.
- `400 Bad Request` para datos inválidos.
- `401 Unauthorized` para usuario no autenticado.
- `403 Forbidden` para acceso a datos de otro negocio.
- `404 Not Found` para recurso inexistente.
- `409 Conflict` para código interno duplicado o conflicto de negocio.

---

## 15. Docker y SQL Server

El proyecto debe incluir un archivo `docker-compose.yml` para levantar SQL Server localmente.

Configuración base esperada:

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: stockflow-sqlserver
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "${SQLSERVER_SA_PASSWORD}"
    ports:
      - "1433:1433"
    volumes:
      - stockflow_sqlserver_data:/var/opt/mssql

volumes:
  stockflow_sqlserver_data:
```

Cadena de conexión local esperada:

```text
Server=localhost,1433;Database=StockFlowDb;User Id=sa;Password=<local-secret>;TrustServerCertificate=True;
```

Reglas:

- La contraseña debe vivir fuera del repositorio, por ejemplo en `.env` para Docker y `dotnet user-secrets` para la API.
- No reutilizar una contraseña local en producción.
- No subir secretos reales.
- Documentar cómo levantar SQL Server con Docker.
- La API debe poder conectarse a SQL Server usando `dotnet user-secrets` o variables de entorno.

Comando esperado para levantar SQL Server:

```bash
docker compose up -d
```

---

## 16. Reglas de Entity Framework Core

Usar Entity Framework Core para persistencia.

Reglas:

- Crear `AppDbContext` en `StockFlow.Infrastructure/Data`.
- Configurar entidades usando Fluent API cuando sea necesario.
- Usar migraciones.
- Configurar precisión decimal para dinero.
- Configurar índices únicos.
- Configurar relaciones correctamente.
- No usar lazy loading por defecto.
- Usar consultas explícitas y claras.
- Usar `AsNoTracking` para consultas de solo lectura cuando aplique.

Precisión recomendada para dinero:

```text
decimal(18,2)
```

Índices obligatorios:

```text
Users.Email unique
Products.BusinessId + Products.InternalCode unique
```

---

## 17. Reglas de pruebas

Usar xUnit.

Todo módulo crítico debe tener pruebas.

---

### 17.1 Pruebas de productos

Casos mínimos:

- Crear producto correctamente.
- Rechazar producto sin nombre.
- Rechazar producto con código interno duplicado dentro del mismo negocio.
- Rechazar producto con precio de venta inválido.
- Rechazar producto con precio de venta menor al precio de compra.
- Rechazar producto con stock negativo.
- Listar productos con bajo stock.
- Listar productos próximos a caducar.
- Listar productos caducados.

---

### 17.2 Pruebas de inventario

Casos mínimos:

- Registrar entrada de inventario.
- Registrar salida de inventario.
- Rechazar movimiento con cantidad menor o igual a cero.
- Rechazar salida que deje stock negativo.
- Registrar historial de movimientos por producto.
- Validar que movimientos respeten `BusinessId`.

---

### 17.3 Pruebas de ventas

Casos mínimos:

- Crear venta correctamente.
- Rechazar venta sin productos.
- Rechazar venta con producto inactivo.
- Rechazar venta con producto caducado.
- Rechazar venta con stock insuficiente.
- Calcular total de venta en backend.
- Calcular ganancia estimada de venta.
- Descontar stock después de una venta.
- Generar movimiento de inventario tipo `Sale`.
- Validar que una venta falle completamente si un producto no cumple reglas.

---

### 17.4 Pruebas de reportes

Casos mínimos:

- Obtener productos con bajo stock.
- Obtener productos próximos a caducar.
- Obtener productos caducados.
- Obtener productos más vendidos.
- Obtener resumen de ventas por rango de fechas.
- Obtener resumen de ganancias por rango de fechas.
- Calcular valoración del inventario.
- Validar que reportes filtren por `BusinessId`.

---

### 17.5 Pruebas de seguridad

Casos mínimos:

- No permitir acceso sin token a endpoints privados.
- No devolver `PasswordHash`.
- No permitir acceso a datos de otro negocio.
- Rechazar credenciales inválidas.

---

## 18. Reglas de documentación

Todo cambio importante debe actualizar documentación.

Archivos principales:

```text
README.md
AGENTS.md
docs/reglas-negocio.md
docs/modelo-base-datos.md
docs/endpoints-api.md
docs/decisiones-tecnicas.md
docs/roadmap-desarrollo.md
docs/seguridad.md
docs/estrategia-pruebas.md
```

Documentar:

- Qué se construyó.
- Por qué se construyó de esa forma.
- Qué reglas aplica.
- Qué endpoints existen.
- Qué validaciones son importantes.
- Qué queda pendiente.
- Qué decisiones técnicas se tomaron.

No crear funcionalidades importantes sin documentarlas.

---

## 19. Reglas de Git

Usar commits claros, pequeños y relacionados con una sola intención.

Formato recomendado:

```text
tipo: descripción corta
```

Tipos permitidos:

```text
feat
fix
docs
test
refactor
chore
build
ci
```

Ejemplos:

```text
docs: agregar documentación base del proyecto
chore: crear estructura inicial de la solución
build: agregar docker compose para sql server
feat: agregar entidades principales del dominio
feat: implementar autenticación con jwt
feat: implementar gestión de productos
fix: evitar venta de productos caducados
test: agregar pruebas de creación de ventas
refactor: mover validaciones de producto a application
```

No hacer commits enormes con cambios no relacionados.

---

## 20. Orden obligatorio de implementación

El proyecto debe construirse por fases.

No saltar fases sin instrucción explícita.

---

### Fase 1: Documentación base

Objetivo:

Definir el contrato del proyecto antes de programar.

Tareas:

- Crear `README.md`.
- Crear `AGENTS.md`.
- Crear carpeta `docs`.
- Crear documentación base.
- Definir alcance de versión 1.
- Definir qué no incluye versión 1.

---

### Fase 2: Solución .NET y estructura base

Objetivo:

Crear la base técnica del proyecto.

Tareas:

- Crear `StockFlow.sln`.
- Crear proyectos:
  - `StockFlow.Api`
  - `StockFlow.Application`
  - `StockFlow.Domain`
  - `StockFlow.Infrastructure`
  - `StockFlow.UnitTests`
  - `StockFlow.IntegrationTests`

- Configurar referencias entre proyectos.
- Configurar Swagger.
- Configurar `.gitignore`.
- Verificar que compile.

No implementar lógica de negocio todavía.

---

### Fase 3: Docker con SQL Server

Objetivo:

Preparar la base de datos local.

Tareas:

- Crear `docker-compose.yml`.
- Configurar SQL Server.
- Documentar cadena de conexión local.
- Verificar conexión desde la API.

---

### Fase 4: Modelo de dominio

Objetivo:

Crear el núcleo del negocio.

Entidades:

- User
- Business
- Category
- Product
- InventoryMovement
- Sale
- SaleItem

Enums:

- InventoryMovementType
- PaymentMethod

Opcional si se necesita:

- Entity base con `Id`, `CreatedAt`, `UpdatedAt`.

---

### Fase 5: Infraestructura y Entity Framework Core

Objetivo:

Configurar persistencia.

Tareas:

- Crear `AppDbContext`.
- Configurar entidades con Fluent API.
- Configurar relaciones.
- Configurar índices únicos.
- Configurar precisión decimal.
- Crear migración inicial.
- Aplicar migración a SQL Server.

---

### Fase 6: Autenticación y negocio

Objetivo:

Implementar acceso seguro.

Tareas:

- Registro de usuario.
- Creación del negocio asociado.
- Login.
- JWT.
- Endpoint `/api/auth/me`.
- Endpoint `/api/businesses/me`.
- Hash de contraseña.
- Protección de endpoints privados.

---

### Fase 7: Categorías

Objetivo:

Implementar organización de productos.

Tareas:

- Crear categoría.
- Listar categorías del negocio.
- Consultar categoría por ID y BusinessId.
- Actualizar categoría.
- Desactivar categoría.
- Agregar pruebas.
- Documentar endpoints.

---

### Fase 8: Productos

Objetivo:

Implementar catálogo de productos.

Tareas:

- Crear producto.
- Listar productos del negocio.
- Buscar por nombre o código.
- Filtrar por categoría.
- Consultar producto por ID y BusinessId.
- Actualizar producto.
- Eliminar si no tiene historial.
- Desactivar si tiene historial.
- Calcular ganancia por unidad.
- Calcular margen de ganancia.
- Consultar stock bajo.
- Consultar próximos a caducar.
- Consultar caducados.
- Agregar pruebas.

---

### Fase 9: Inventario

Objetivo:

Registrar cambios de stock correctamente.

Tareas:

- Registrar entrada.
- Registrar salida.
- Registrar ajuste.
- Validar stock suficiente.
- Evitar stock negativo.
- Registrar historial por producto.
- Filtrar por BusinessId.
- Agregar pruebas.

---

### Fase 10: Ventas

Objetivo:

Crear ventas conectadas con inventario.

Tareas:

- Crear venta.
- Crear detalle de venta.
- Validar productos.
- Validar stock.
- Validar producto activo.
- Validar producto no caducado.
- Calcular total.
- Calcular ganancia.
- Descontar stock.
- Crear movimientos de inventario.
- Usar transacción.
- Agregar pruebas.

---

### Fase 11: Reportes puntuales

Objetivo:

Crear solo reportes útiles para negocio.

Reportes permitidos:

- Productos con bajo stock.
- Productos próximos a caducar.
- Productos caducados.
- Productos más vendidos.
- Resumen de ventas por rango de fechas.
- Resumen de ganancias por rango de fechas.
- Valoración del inventario.

No crear otros reportes sin instrucción explícita.

---

### Fase 12: Calidad final

Objetivo:

Preparar el proyecto para portafolio.

Tareas:

- Completar pruebas.
- Mejorar README.
- Completar documentación.
- Agregar ejemplos de uso.
- Agregar capturas de Swagger.
- Agregar diagrama de base de datos.
- Revisar nombres.
- Revisar seguridad.
- Revisar endpoints.

---

## 21. Definición de terminado

Una funcionalidad se considera terminada solo cuando:

- Compila correctamente.
- Respeta arquitectura por capas.
- No tiene lógica de negocio en controladores.
- No usa `DbContext` directamente desde controladores.
- Usa DTOs.
- Valida entradas.
- Maneja errores.
- Respeta `BusinessId`.
- Respeta reglas de negocio.
- Incluye pruebas cuando aplica.
- Actualiza documentación.
- No rompe pruebas existentes.
- Usa nombres claros.
- No agrega complejidad innecesaria.

---

## 22. Prácticas prohibidas

No hacer lo siguiente:

- Poner toda la lógica en controladores.
- Usar `DbContext` directamente en controladores.
- Exponer entidades como respuestas públicas.
- Saltarse validaciones.
- Ignorar reglas de negocio.
- Permitir stock negativo.
- Vender productos caducados.
- Vender productos inactivos.
- Confiar en totales enviados desde el cliente.
- Crear ventas sin transacción.
- Eliminar movimientos de inventario.
- Eliminar ventas físicamente.
- Permitir acceso a datos de otro negocio.
- Buscar recursos operativos solo por `Id` sin validar `BusinessId`.
- Crear reportes innecesarios.
- Agregar caja en versión 1.
- Agregar tecnologías no aprobadas.
- Crear funcionalidades fuera del alcance.
- Dejar código muerto.
- Dejar TODOs sin explicación.
- Crear arquitectura innecesariamente compleja.
- Implementar todo el proyecto en un solo paso.

---

## 23. Comportamiento esperado de la IA

Cuando una IA trabaje en este proyecto, debe:

1. Leer `AGENTS.md` antes de modificar código.
2. Leer `README.md` antes de modificar el alcance.
3. Respetar la arquitectura definida.
4. Trabajar en pasos pequeños y verificables.
5. Explicar el plan antes de cambios grandes.
6. No inventar requisitos fuera del alcance.
7. Preguntar si una regla de negocio es ambigua.
8. Crear código simple, claro y mantenible.
9. Agregar pruebas para lógica crítica.
10. Actualizar documentación cuando cambie comportamiento.
11. Resumir los cambios realizados.
12. Indicar riesgos o tareas pendientes.
13. No avanzar a otro módulo sin cerrar el actual.

---

## 24. Prompt recomendado para trabajar con IA

Usar este formato al pedir cambios:

```text
Lee y respeta AGENTS.md y README.md.

Implementa únicamente la fase [NOMBRE DE LA FASE].

No agregues funcionalidades fuera de esa fase.
No cambies tecnologías.
No modifiques módulos no relacionados.

Al finalizar:
- Verifica que compile.
- Ejecuta pruebas si existen.
- Actualiza documentación si corresponde.
- Resume los cambios realizados.
- Indica pendientes o riesgos.
```

---

## 25. Estado actual del proyecto

Estado:

```text
Base funcional de version 1 implementada con solucion .NET, capas, JWT, EF Core, SQL Server, Swagger, Docker Compose, migracion inicial, pruebas y documentacion tecnica.
```

Siguiente paso recomendado:

```text
Ampliar cobertura automatizada, endurecer validaciones de entrada y profundizar documentacion/examples para portafolio.
```
