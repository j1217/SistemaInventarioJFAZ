# Interfaz web

Aplicación Angular 20 para autenticación y gestión del inventario. Usa componentes standalone, servicios para la API, un interceptor JWT y una guardia de rutas.

## Requisitos

- Node.js 22 o superior.
- API disponible en `http://localhost:5080`.

## Ejecución local

Desde esta carpeta ejecuta:

```bash
npm ci
npm start
```

Abre `http://localhost:4200`. Las credenciales de demostración se configuran en el backend y se documentan en el README de la raíz.

## Funciones

- Consultar productos, existencias actuales y movimientos recientes.
- Buscar productos y filtrar por disponibilidad.
- Crear productos y registrar entradas o salidas.
- Mantener la sesión mediante el JWT de la API.

La interfaz obtiene los datos del backend; los errores de la API se muestran en la pantalla y no se guardan datos de inventario en el navegador.
