using Microsoft.Data.SqlClient;
using ProductosApi.Models;

namespace ProductosApi.Data;

public class ProductoRepository : IProductoRepository
{
    private readonly string _connectionString;

    public ProductoRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection'.");
    }

    private SqlConnection CrearConexion() => new(_connectionString);

    public List<Producto> ObtenerTodos()
    {
        using var conexion = CrearConexion();
        using var comando = new SqlCommand(
            "SELECT Id, Nombre, Descripcion, Precio, Stock FROM Productos ORDER BY Id", conexion);
        conexion.Open();
        return LeerProductos(comando);
    }

    public Producto? ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();
        using var comando = new SqlCommand(
            "SELECT Id, Nombre, Descripcion, Precio, Stock FROM Productos WHERE Id = @Id", conexion);
        comando.Parameters.AddWithValue("@Id", id);
        conexion.Open();
        return LeerProductos(comando).FirstOrDefault();
    }

    public int Insertar(Producto producto)
    {
        using var conexion = CrearConexion();
        using var comando = new SqlCommand(
            @"INSERT INTO Productos (Nombre, Descripcion, Precio, Stock)
              OUTPUT INSERTED.Id
              VALUES (@Nombre, @Descripcion, @Precio, @Stock)", conexion);
        AgregarParametros(comando, producto);
        conexion.Open();
        return (int)comando.ExecuteScalar();
    }

    public bool Actualizar(Producto producto)
    {
        using var conexion = CrearConexion();
        using var comando = new SqlCommand(
            @"UPDATE Productos
              SET Nombre = @Nombre, Descripcion = @Descripcion, Precio = @Precio, Stock = @Stock
              WHERE Id = @Id", conexion);
        AgregarParametros(comando, producto);
        comando.Parameters.AddWithValue("@Id", producto.Id);
        conexion.Open();
        return comando.ExecuteNonQuery() > 0;
    }

    public bool Eliminar(int id)
    {
        using var conexion = CrearConexion();
        using var comando = new SqlCommand("DELETE FROM Productos WHERE Id = @Id", conexion);
        comando.Parameters.AddWithValue("@Id", id);
        conexion.Open();
        return comando.ExecuteNonQuery() > 0;
    }

    public List<Producto> Buscar(string texto)
    {
        using var conexion = CrearConexion();
        using var comando = new SqlCommand(
            @"SELECT Id, Nombre, Descripcion, Precio, Stock FROM Productos
              WHERE Nombre LIKE @Texto OR Descripcion LIKE @Texto
              ORDER BY Id", conexion);
        comando.Parameters.AddWithValue("@Texto", $"%{texto}%");
        conexion.Open();
        return LeerProductos(comando);
    }

    private static void AgregarParametros(SqlCommand comando, Producto producto)
    {
        comando.Parameters.AddWithValue("@Nombre", producto.Nombre);
        comando.Parameters.AddWithValue("@Descripcion", (object?)producto.Descripcion ?? DBNull.Value);
        comando.Parameters.AddWithValue("@Precio", producto.Precio);
        comando.Parameters.AddWithValue("@Stock", producto.Stock);
    }

    private static List<Producto> LeerProductos(SqlCommand comando)
    {
        var productos = new List<Producto>();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            productos.Add(new Producto
            {
                Id = lector.GetInt32(0),
                Nombre = lector.GetString(1),
                Descripcion = lector.IsDBNull(2) ? null : lector.GetString(2),
                Precio = lector.GetDecimal(3),
                Stock = lector.GetInt32(4)
            });
        }
        return productos;
    }
}
