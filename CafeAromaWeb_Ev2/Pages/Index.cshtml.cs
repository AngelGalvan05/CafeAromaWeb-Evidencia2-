using CafeAromaWeb.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CafeAromaWeb.Pages;

/// <summary>Página de Inicio: saludo según la hora, fecha actual y productos destacados.</summary>
public class IndexModel : PageModel
{
    public string Saludo { get; private set; } = "";
    public DateTime Fecha { get; private set; }
    public List<Producto> Destacados { get; private set; } = new();

    public void OnGet()
    {
        Fecha = DateTime.Now;
        Saludo = ObtenerSaludo(Fecha.Hour);
        Destacados = Catalogo.Productos.Take(3).ToList();
    }

    private static string ObtenerSaludo(int hora)
    {
        return hora switch
        {
            < 12 => "Buenos días",
            < 19 => "Buenas tardes",
            _    => "Buenas noches"
        };
    }
}
