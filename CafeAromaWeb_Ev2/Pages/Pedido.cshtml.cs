using System.ComponentModel.DataAnnotations;
using CafeAromaWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CafeAromaWeb.Pages;

/// <summary>
/// Página Pedido: formulario de negocio. Recibe los datos del cliente, los valida,
/// calcula el total y muestra el resumen del pedido.
/// </summary>
public class PedidoModel : PageModel
{
    // ----- Reglas de precio (una sola fuente; la vista las envía también al cliente) -----
    public static readonly string[] Tamaños = { "Chico", "Mediano", "Grande" };
    public static readonly Dictionary<string, decimal> RecargosTamaño = new()
    {
        ["Chico"] = 0m,
        ["Mediano"] = 10m,
        ["Grande"] = 20m
    };

    public static readonly string[] ExtrasDisponibles = { "Leche de almendra", "Shot extra de café", "Crema batida" };
    public const decimal PrecioExtra = 10m;   
    public const decimal CostoEnvio = 30m;    

    [BindProperty]
    public PedidoDatos Datos { get; set; } = new();

    public List<SelectListItem> ListaProductos { get; } = Catalogo.Productos
        .Select(p => new SelectListItem($"{p.Nombre} - {p.Precio:C}", p.Id.ToString()))
        .ToList();


    public Dictionary<int, decimal> PreciosProductos { get; } =
        Catalogo.Productos.ToDictionary(p => p.Id, p => p.Precio);


    public Producto? ProductoElegido { get; private set; }
    public decimal? Total { get; private set; }

    /// <summary>
    /// Se ejecuta al abrir la página (GET). Si llega ?producto=Id desde la página Inicio,
    /// ese producto aparece preseleccionado en la lista.
    /// </summary>
    public void OnGet(int? producto)
    {
        if (producto is not null && Catalogo.Buscar(producto.Value) is not null)
        {
            Datos.ProductoId = producto;
        }
    }


    public IActionResult OnPost()
    {
        Datos.Extras = DepurarExtras(Datos.Extras);
        if (Datos.Tamaño is not null && !Tamaños.Contains(Datos.Tamaño))
        {
            ModelState.AddModelError("Datos.Tamaño", "Seleccione un tamaño válido.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        ProductoElegido = Catalogo.Buscar(Datos.ProductoId!.Value);
        if (ProductoElegido is null)
        {
            ModelState.AddModelError("Datos.ProductoId", "El producto no existe.");
            return Page();
        }

        Datos.Nombre = Datos.Nombre.Trim();
        Total = CalcularTotal(ProductoElegido, Datos);
        return Page();
    }

    /// <summary>Función definida por el usuario: conserva solo los extras de la lista oficial, sin repetir.</summary>
    private static List<string> DepurarExtras(List<string>? extras)
    {
        return (extras ?? new()).Where(e => ExtrasDisponibles.Contains(e)).Distinct().ToList();
    }

    /// <summary>
    /// Función definida por el usuario: calcula el total del pedido.
    /// Por unidad: precio + recargo por tamaño + $10 por cada extra. Si hay entrega, se suman $30.
    /// </summary>
    public static decimal CalcularTotal(Producto producto, PedidoDatos datos)
    {
        decimal recargoTamaño = RecargosTamaño.GetValueOrDefault(datos.Tamaño ?? "", 0m);
        decimal recargoExtras = datos.Extras.Count * PrecioExtra;
        decimal subtotal = (producto.Precio + recargoTamaño + recargoExtras) * datos.Cantidad!.Value;
        decimal envio = datos.EntregaDomicilio ? CostoEnvio : 0m;
        return subtotal + envio;
    }
}

/// <summary>Datos capturados en el formulario de Pedido, con sus reglas de validación.</summary>
public class PedidoDatos
{
    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(60, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 60 caracteres.")]
    public string Nombre { get; set; } = "";

    [Display(Name = "Correo electrónico")]
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
    public string Correo { get; set; } = "";

    [Display(Name = "Teléfono")]
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "El teléfono debe tener 10 dígitos.")]
    public string Telefono { get; set; } = "";


    [Display(Name = "Producto")]
    [Required(ErrorMessage = "Seleccione un producto.")]
    public int? ProductoId { get; set; }

    [Display(Name = "Cantidad")]
    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(1, 10, ErrorMessage = "La cantidad debe estar entre 1 y 10.")]
    public int? Cantidad { get; set; } = 1;


    [Display(Name = "Tamaño")]
    [Required(ErrorMessage = "Seleccione un tamaño.")]
    public string? Tamaño { get; set; }


    public List<string> Extras { get; set; } = new();

    [Display(Name = "Entrega a domicilio")]
    public bool EntregaDomicilio { get; set; }

    [Display(Name = "Dirección de entrega")]
    [RequeridoSi(nameof(EntregaDomicilio), ErrorMessage = "La dirección es obligatoria para entrega a domicilio.")]
    [StringLength(120, ErrorMessage = "La dirección no puede exceder 120 caracteres.")]
    public string? Direccion { get; set; }
}
