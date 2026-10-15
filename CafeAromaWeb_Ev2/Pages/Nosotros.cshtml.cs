using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CafeAromaWeb.Pages;

/// <summary>Página Nosotros: historia, misión y visión. Calcula los años de servicio.</summary>
public class NosotrosModel : PageModel
{
    /// <summary>Año en que abrió la cafetería (dato ficticio).</summary>
    public const int AnioFundacion = 2018;

    public int AniosServicio { get; private set; }

    public void OnGet()
    {
        AniosServicio = DateTime.Now.Year - AnioFundacion;
    }
}
