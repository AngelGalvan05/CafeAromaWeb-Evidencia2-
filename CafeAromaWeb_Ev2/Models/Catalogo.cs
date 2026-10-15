namespace CafeAromaWeb.Models;

/// <summary>Producto del menú de la cafetería.</summary>
/// <param name="Id">Identificador único del producto.</param>
/// <param name="Nombre">Nombre que se muestra al cliente.</param>
/// <param name="Categoria">Categoría para agrupar el menú (Bebidas, Alimentos).</param>
/// <param name="Precio">Precio base en pesos mexicanos (tamaño chico).</param>
/// <param name="Descripcion">Descripción breve del producto.</param>
public record Producto(int Id, string Nombre, string Categoria, decimal Precio, string Descripcion);

/// <summary>
/// Catálogo único de productos: lo usan las páginas Inicio, Menú y Pedido,
/// de modo que un cambio de precio se refleja en todo el sitio.
/// </summary>
public static class Catalogo
{
    public static readonly List<Producto> Productos = new()
    {
        new(1, "Café americano", "Bebidas",   35m, "Espresso diluido en agua caliente, suave y aromático."),
        new(2, "Capuchino",      "Bebidas",   45m, "Espresso con leche vaporizada y espuma cremosa."),
        new(3, "Latte",          "Bebidas",   50m, "Espresso con abundante leche y una capa ligera de espuma."),
        new(4, "Té chai",        "Bebidas",   40m, "Té negro con canela, cardamomo y jengibre."),
        new(5, "Pan dulce",      "Alimentos", 25m, "Concha o cuerno horneado cada mañana."),
        new(6, "Cheesecake",     "Alimentos", 55m, "Rebanada de pay de queso con coulis de frutos rojos.")
    };

    /// <summary>Función definida por el usuario: busca un producto por su identificador.</summary>
    /// <returns>El producto encontrado o <c>null</c> si el identificador no existe.</returns>
    public static Producto? Buscar(int id)
    {
        return Productos.FirstOrDefault(p => p.Id == id);
    }
}
