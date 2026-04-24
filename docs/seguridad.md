# Seguridad

## Autenticacion

- JWT Bearer configurado en `src/StockFlow.Api/Program.cs`.
- El token exige issuer, audience, firma y expiracion.
- El token incluye `userId` y `businessId` para resolver contexto del negocio.

## Contrasenas

- Se usa `PasswordHasher<User>`.
- No se retorna `PasswordHash` en respuestas.
- `login` responde mensaje generico ante credenciales invalidas.

## Aislamiento de negocio

- `HttpUserContext` resuelve el negocio autenticado desde claims.
- Los servicios usan ese contexto para toda consulta operativa.
- Las pruebas de integracion validan acceso denegado entre negocios.

## Manejo de errores

- `ExceptionHandlingMiddleware` transforma errores de dominio en codigos HTTP controlados.
- No se devuelve stack trace al cliente.

## Configuracion

- `appsettings*.json` usa valores de desarrollo local.
- Docker y base local usan credenciales claramente marcadas como desarrollo.
- No hay secretos productivos versionados.
