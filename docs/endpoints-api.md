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
- `GET /api/categories`
- `GET /api/categories/{id}`
- `PUT /api/categories/{id}`
- `PATCH /api/categories/{id}/deactivate`

## Products

- `POST /api/products`
- `GET /api/products`
- `GET /api/products/{id}`
- `GET /api/products/search?term=value`
- `GET /api/products/low-stock`
- `GET /api/products/expiring-soon?days=30`
- `GET /api/products/expired`
- `PUT /api/products/{id}`
- `PATCH /api/products/{id}/deactivate`
- `DELETE /api/products/{id}`

## Inventory

- `POST /api/inventory/movements`
- `GET /api/inventory/movements`
- `GET /api/inventory/products/{productId}/history`

## Sales

- `POST /api/sales`
- `GET /api/sales`
- `GET /api/sales/{id}`
- `GET /api/sales/by-date?from=2026-01-01&to=2026-01-31`

## Reports

- `GET /api/reports/low-stock`
- `GET /api/reports/expiring-soon?days=30`
- `GET /api/reports/expired-products`
- `GET /api/reports/top-selling-products`
- `GET /api/reports/sales-summary?from=2026-01-01&to=2026-01-31`
- `GET /api/reports/profit-summary?from=2026-01-01&to=2026-01-31`
- `GET /api/reports/inventory-valuation`

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
