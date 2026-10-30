# ProductosApi – Acceso a base de datos con ADO.NET

Backend en ASP.NET Core (.NET 10) que realiza operaciones CRUD sobre la entidad **Producto** usando **ADO.NET** puro (`Microsoft.Data.SqlClient`) contra **SQL Server**, y se prueba con **Swagger**.

## Cómo ejecutarlo

1. Crear la base de datos y la tabla ejecutando el script:
   ```
   sqlcmd -S localhost -E -f 65001 -i database.sql
   ```
   (o abrir `database.sql` en SQL Server Management Studio y ejecutarlo).
2. Revisar la cadena de conexión en `appsettings.json` (`ConnectionStrings:DefaultConnection`).
3. Ejecutar el proyecto:
   ```
   dotnet run
   ```
4. Abrir `http://localhost:5192/swagger` para probar los endpoints.

## Estructura

```
ProductosApi/
├── Controllers/ProductosController.cs   -> Endpoints HTTP
├── Data/IProductoRepository.cs          -> Interface solicitada
├── Data/ProductoRepository.cs           -> Implementación con ADO.NET
├── Models/Producto.cs                   -> Entidad
├── Program.cs                           -> Configuración (DI, Swagger)
├── appsettings.json                     -> Cadena de conexión
└── database.sql                         -> Script de la BD
```

## Endpoints

| Método | Ruta                               | Punto a resolver | Método del repositorio |
|--------|------------------------------------|------------------|------------------------|
| GET    | `/api/productos`                   | Consultas        | `ObtenerTodos()`       |
| GET    | `/api/productos/{id}`              | Consultas        | `ObtenerPorId(id)`     |
| GET    | `/api/productos/buscar?texto=...`  | Buscar           | `Buscar(texto)`        |
| POST   | `/api/productos`                   | Insert           | `Insertar(producto)`   |
| PUT    | `/api/productos/{id}`              | Actualizar       | `Actualizar(producto)` |
| DELETE | `/api/productos/{id}`              | Eliminar         | `Eliminar(id)`         |

## Flujo del proyecto

Cuando alguien hace una petición (por ejemplo desde Swagger), el recorrido es este:

1. **Program.cs** arranca la aplicación. Ahí se registran los controladores, Swagger y la inyección de dependencias: cada vez que algo pida un `IProductoRepository`, el sistema le entrega un `ProductoRepository`.
2. La petición llega al **ProductosController**, que según la ruta y el verbo HTTP (GET, POST, PUT, DELETE) decide qué método ejecutar. El controlador no sabe nada de SQL; solo recibe el repositorio por el constructor y le pide lo que necesita.
3. El **ProductoRepository** es el que habla con la base de datos usando ADO.NET:
   - **Conexión:** lee la cadena `DefaultConnection` desde `appsettings.json` y crea un `SqlConnection` en cada operación. Con `using` la conexión se cierra sola al terminar.
   - **SqlCommand:** se arma la sentencia SQL con **parámetros** (`@Id`, `@Nombre`, etc.) en lugar de concatenar texto, para evitar inyección SQL.
   - **Consultas / Buscar:** se usa `ExecuteReader()` y un `SqlDataReader` que recorre las filas una por una; cada fila se convierte en un objeto `Producto`. Buscar usa `LIKE` para encontrar coincidencias en el nombre o la descripción.
   - **Insert:** se usa `ExecuteScalar()` junto con `OUTPUT INSERTED.Id` para obtener el Id que generó la base de datos.
   - **Actualizar / Eliminar:** se usa `ExecuteNonQuery()`, que regresa cuántas filas se afectaron; si es mayor a 0 la operación fue exitosa (`true`), si no, el producto no existía (`false`).
4. El controlador recibe el resultado y responde con el código HTTP adecuado: `200 OK` con los datos, `201 Created` al insertar, `204 No Content` al actualizar o eliminar, y `404 Not Found` si el producto no existe.
5. **Swagger** lee los controladores y genera automáticamente una página donde se pueden probar todos los endpoints sin necesidad de otra herramienta.
