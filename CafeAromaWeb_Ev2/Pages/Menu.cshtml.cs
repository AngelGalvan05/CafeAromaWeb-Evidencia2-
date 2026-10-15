using CafeAromaWeb.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CafeAromaWeb.Pages;

/// <summary>Página Menú: muestra todo el catálogo agrupado por categoría.</summary>
public class MenuModel : PageModel
{
    public List<Producto> Productos { get; private set; } = new();

    /// <summary>Categorías distintas del catálogo; llenan la lista desplegable de filtro.</summary>
    public List<string> Categorias { get; private set; } = new();

    public void OnGet()
    {
        Productos = Catalogo.Productos;
        Categorias = Productos.Select(p => p.Categoria).Distinct().ToList();
    }
}
