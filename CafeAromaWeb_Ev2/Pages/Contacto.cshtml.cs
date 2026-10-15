using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CafeAromaWeb.Pages;

/// <summary>Página Contacto: datos de la cafetería y aviso de si está abierta.</summary>
public class ContactoModel : PageModel
{

    public const int HoraApertura = 8;
    public const int HoraCierre = 20;

    public bool Abierto { get; private set; }
    public DateTime Ahora { get; private set; }

    public void OnGet()
    {
        Ahora = DateTime.Now;
        Abierto = EstaAbierto(Ahora);
    }

    /// <summary>
    /// Función definida por el usuario: indica si la hora recibida está dentro del horario
    /// de servicio (desde HoraApertura inclusive hasta HoraCierre exclusive).
    /// </summary>
    public static bool EstaAbierto(DateTime ahora)
    {
        return ahora.Hour >= HoraApertura && ahora.Hour < HoraCierre;
    }
}
