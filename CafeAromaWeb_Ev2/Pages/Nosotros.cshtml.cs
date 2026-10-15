using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CafeAromaWeb.Pages;

/// <summary>Página Nosotros: historia, misión y visión. Calcula los años de servicio.</summary>
public class NosotrosModel : PageModel
{

    public const int AñoFundacion = 2026;

    public int AñosServicio { get; private set; }

    public void OnGet()
    {
        AñosServicio = DateTime.Now.Year - AñoFundacion;
    }
}
