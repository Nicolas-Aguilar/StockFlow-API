# Endpoints API

## Auth

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`

## Businesses

- `GET /api/businesses/me`
- `PUT /api/businesses/me`

## Categories

- `POST /api/categories`
- `GET /api/categories?page=1&pageSize=20`
- `GET /api/categories/{id}`
- `PUT /api/categories/{id}`
- `PATCH /api/categories/{id}/deactivate`

## Products

- `POST /api/products`
- `GET /api/products?page=1&pageSize=20&categoryId={guid}`
- `GET /api/products/{id}`
- `GET /api/products/search?term=value&page=1&pageSize=20`
- `GET /api/products/low-stock?page=1&pageSize=20`
- `GET /api/products/expiring-soon?days=30&page=1&pageSize=20`
- `GET /api/products/expired?page=1&pageSize=20`
- `PUT /api/products/{id}`
- `PATCH /api/products/{id}/deactivate`
- `DELETE /api/products/{id}`

## Inventory

- `POST /api/inventory/movements`
- `GET /api/inventory/movements?page=1&pageSize=20`
- `GET /api/inventory/products/{productId}/history?page=1&pageSize=20`

## Sales

- `POST /api/sales`
- `GET /api/sales?page=1&pageSize=20`
- `GET /api/sales/{id}`
- `GET /api/sales/by-date?from=2026-01-01&to=2026-01-31&page=1&pageSize=20`

## Reports

- `GET /api/reports/low-stock?page=1&pageSize=20`
- `GET /api/reports/expiring-soon?days=30&page=1&pageSize=20`
- `GET /api/reports/expired-products?page=1&pageSize=20`
- `GET /api/reports/top-selling-products?page=1&pageSize=20`
- `GET /api/reports/sales-summary?from=2026-01-01&to=2026-01-31`
- `GET /api/reports/profit-summary?from=2026-01-01&to=2026-01-31`
- `GET /api/reports/inventory-valuation`

## Contrato de paginacion

- Los endpoints listados arriba devuelven un objeto con `items`, `page`, `pageSize`, `totalItems` y `totalPages`.
- `page` debe ser mayor o igual a `1`.
- `pageSize` debe estar entre `1` y `100`.
- Los endpoints agregados (`sales-summary`, `profit-summary`, `inventory-valuation`) mantienen su respuesta actual sin envoltura paginada.

## Contrato de errores

- Los errores controlados usan `application/problem+json`.
- Siempre incluyen `type`, `title`, `status`, `detail` y `traceId`.
- Los errores de validacion usan `ValidationProblemDetails`.
- Claims faltantes o invalidos en un JWT autenticado responden `401 Unauthorized`; ya no caen en `Guid.Empty`.

## Autenticacion

- Todos los endpoints salvo `register` y `login` requieren bearer token.
- El JWT incluye `userId` y `businessId`.

## Codigos HTTP usados

- `200 OK`
- `201 Created`
- `204 No Content`
- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict`
- `500 Internal Server Error`
