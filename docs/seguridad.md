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
- Si `userId` o `businessId` faltan o son invalidos, la API corta la solicitud con `401 Unauthorized`.
- Los servicios usan ese contexto para toda consulta operativa.
- Las pruebas de integracion validan acceso denegado entre negocios.

## Manejo de errores

- `ExceptionHandlingMiddleware` transforma errores de dominio y de contexto autenticado en `ProblemDetails` / `ValidationProblemDetails`.
- Todas las respuestas de error controladas incluyen `traceId`.
- No se devuelve stack trace al cliente.

## Configuracion

- `src/StockFlow.Api/appsettings.json` conserva solo placeholders y configuracion no sensible.
- `src/StockFlow.Api/appsettings.Development.json` ya no replica secretos; solo redefine logging de desarrollo.
- La API usa `dotnet user-secrets` como mecanismo recomendado para `ConnectionStrings:DefaultConnection` y `Jwt:Key` en desarrollo local.
- Docker Compose toma `SQLSERVER_SA_PASSWORD` desde `.env`, y `.env.example` documenta los nombres requeridos sin exponer credenciales reales.
- `scripts/bootstrap.ps1` y `scripts/bootstrap.sh` generan credenciales locales solo cuando faltan y las guardan fuera del repositorio versionado.
- `AppDbContextFactory` consume la misma jerarquia de configuracion que la API para evitar secretos hardcodeados en migraciones.
- Las credenciales que estuvieron versionadas historicamente deben tratarse como comprometidas y rotarse si se reutilizaron fuera del entorno local.
