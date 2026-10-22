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

1. Copia `.env.example` como `.env` y define contraseñas propias para la base de datos y el usuario administrador, además de un secreto JWT de al menos 32 bytes.
2. Desde la raíz del repositorio ejecuta `docker compose up --build`.
3. Abre `http://localhost:4200`. La documentación de la API está en `http://localhost:5080/swagger`.
4. En una base nueva, el script crea seis productos de muestra. La API crea el administrador y los movimientos iniciales una sola vez.

Para una demostración local, las credenciales iniciales son `admin@inventario.local` / `InventoryDemo2026!`. Cámbialas en `.env` antes de iniciar el entorno.

El script `database/init.sql` se ejecuta automáticamente al crear por primera vez el volumen de PostgreSQL. En una instalación local existente, crea la base y aplica el script con `createdb -h localhost -U postgres inventory` y `psql -h localhost -U postgres -d inventory -f database/init.sql`. Docker no vuelve a ejecutar los scripts de inicialización sobre un volumen ya creado.

## Desarrollo local

1. Ejecuta PostgreSQL, crea la base `inventory` y aplica `database/init.sql` con los comandos anteriores.
2. Configura estas variables para la API: `ConnectionStrings__Default`, `Jwt__Secret` (mínimo 32 bytes), `Jwt__Issuer`, `Jwt__Audience`, `Seed__AdminEmail`, `Seed__AdminPassword` y `FrontendOrigin`.
3. Desde la raíz, ejecuta `dotnet run --project src/Inventory.Api`.
4. En `frontend`, ejecuta `npm ci` y `npm start`. La interfaz usa la API en `http://localhost:5080/api`.

## Pruebas automatizadas

Desde la raíz del repositorio, ejecuta `dotnet test InventoryManagement.sln`. Las pruebas cubren validaciones del producto y reglas del servicio de inventario, incluyendo cantidades inválidas, salidas y límites de consulta.

GitHub Actions ejecuta las pruebas automáticamente en cada push y pull request. También se puede iniciar manualmente desde la pestaña **Actions** del repositorio.

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

Las capturas de la aplicación están disponibles en `docs/screenshots/`:

- [Inicio de sesión](docs/screenshots/inicio-sesion.png)
- [Panel de inventario](docs/screenshots/panel-inventario.png)
- [Registro de producto](docs/screenshots/producto-nuevo.png)
- [Registro de entrada](docs/screenshots/entrada-inventario.png)
- [Registro de salida](docs/screenshots/salida-inventario.png)
