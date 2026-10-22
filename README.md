# Sistema de gestión de inventario

Aplicación web para administrar productos, consultar existencias y registrar entradas y salidas con trazabilidad por usuario.

## Tecnologías

- API: C# / ASP.NET Core 8, Entity Framework Core y PostgreSQL.
- Interfaz: Angular 20.3, TypeScript y SCSS.
- Seguridad: JWT de corta duración, contraseñas con BCrypt y límite de intentos de inicio de sesión.
- Base de datos: procedimientos PostgreSQL para entradas, salidas y consulta del inventario actual.

## Arquitectura

`src/Inventory.Domain` contiene las entidades y reglas del dominio. `src/Inventory.Application` define los casos de uso, contratos y puertos. `src/Inventory.Infrastructure` implementa persistencia, seguridad y adaptadores. `src/Inventory.Api` expone los controladores REST y configura autenticación, validación y errores. La carpeta `frontend` contiene la aplicación Angular.

## Inicio rápido con Docker

1. Copia `.env.example` como `.env` y cambia las contraseñas y el secreto JWT.
2. Desde la raíz del repositorio ejecuta `docker compose up --build`.
3. Abre `http://localhost:4200`. La documentación de la API está en `http://localhost:5080/swagger`.
4. En el primer inicio, el script crea seis productos de muestra. El contenedor de API crea el usuario administrador y movimientos de demostración.

Credenciales de demostración por defecto: `admin@inventario.local` / `InventoryDemo2026!`. Cambia estos valores en `.env` antes de usar el sistema fuera de una demostración local.

El script `database/init.sql` se ejecuta automáticamente al crear por primera vez el volumen de PostgreSQL. Para una base existente, ejecútalo manualmente con `psql` antes de iniciar la API. Los scripts de inicialización de Docker no se vuelven a ejecutar sobre un volumen ya creado.

## Desarrollo local

1. Ejecuta PostgreSQL y aplica `database/init.sql`.
2. Configura estas variables para la API: `ConnectionStrings__Default`, `Jwt__Secret` (mínimo 32 bytes), `Jwt__Issuer`, `Jwt__Audience`, `Seed__AdminEmail`, `Seed__AdminPassword` y `FrontendOrigin`.
3. Desde la raíz, ejecuta `dotnet run --project src/Inventory.Api`.
4. En `frontend`, ejecuta `npm ci` y `npm start`.

## API

| Método | Ruta | Uso |
| --- | --- | --- |
| `POST` | `/api/auth/login` | Iniciar sesión y obtener un JWT |
| `GET` | `/api/products` | Consultar productos |
| `GET` | `/api/products/{id}` | Consultar un producto |
| `POST` | `/api/products` | Crear un producto |
| `GET` | `/api/inventory` | Consultar existencias actuales |
| `GET` | `/api/inventory/movements?take=20` | Consultar movimientos recientes |
| `POST` | `/api/inventory/entries` | Registrar una entrada |
| `POST` | `/api/inventory/exits` | Registrar una salida |

Salvo el inicio de sesión, las rutas requieren `Authorization: Bearer <token>`. Las respuestas de error usan el formato `ProblemDetails`.

## Capturas

Las capturas de la aplicación se guardan en `docs/screenshots/`.
