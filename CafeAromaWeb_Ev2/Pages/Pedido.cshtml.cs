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
    public static readonly string[] Tamanos = { "Chico", "Mediano", "Grande" };

    /// <summary>Recargo por unidad según el tamaño elegido.</summary>
    public static readonly Dictionary<string, decimal> RecargosTamano = new()
    {
        ["Chico"] = 0m,
        ["Mediano"] = 10m,
        ["Grande"] = 20m
    };

    public static readonly string[] ExtrasDisponibles = { "Leche de almendra", "Shot extra de café", "Crema batida" };
    public const decimal PrecioExtra = 10m;   // por cada extra y por unidad
    public const decimal CostoEnvio = 30m;    // cargo fijo por entrega a domicilio

    // Todos los controles del formulario se enlazan a este objeto (model binding).
    [BindProperty]
    public PedidoDatos Datos { get; set; } = new();

    // Elementos de la lista desplegable, generados desde el catálogo.
    public List<SelectListItem> ListaProductos { get; } = Catalogo.Productos
        .Select(p => new SelectListItem($"{p.Nombre} - {p.Precio:C}", p.Id.ToString()))
        .ToList();

    // Precio de cada producto por Id; la vista lo serializa a JSON para el cálculo en el cliente.
    public Dictionary<int, decimal> PreciosProductos { get; } =
        Catalogo.Productos.ToDictionary(p => p.Id, p => p.Precio);

    // Resultado del pedido; son nulos hasta que se envía un formulario válido.
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

    /// <summary>Se ejecuta al enviar el formulario (POST).</summary>
    public IActionResult OnPost()
    {
        // Se ignoran valores que no pertenezcan a las listas oficiales (protección contra datos alterados).
        Datos.Extras = DepurarExtras(Datos.Extras);
        if (Datos.Tamano is not null && !Tamanos.Contains(Datos.Tamano))
        {
            ModelState.AddModelError("Datos.Tamano", "Seleccione un tamaño válido.");
        }

        // La validación del navegador puede omitirse, así que el servidor siempre revisa ModelState.
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
        decimal recargoTamano = RecargosTamano.GetValueOrDefault(datos.Tamano ?? "", 0m);
        decimal recargoExtras = datos.Extras.Count * PrecioExtra;
        decimal subtotal = (producto.Precio + recargoTamano + recargoExtras) * datos.Cantidad!.Value;
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

    // Lista desplegable (select).
    [Display(Name = "Producto")]
    [Required(ErrorMessage = "Seleccione un producto.")]
    public int? ProductoId { get; set; }

    [Display(Name = "Cantidad")]
    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(1, 10, ErrorMessage = "La cantidad debe estar entre 1 y 10.")]
    public int? Cantidad { get; set; } = 1;

    // Botones de opción (radio): una sola selección.
    [Display(Name = "Tamaño")]
    [Required(ErrorMessage = "Seleccione un tamaño.")]
    public string? Tamano { get; set; }

    // Casillas de verificación (checkbox) con varias selecciones.
    public List<string> Extras { get; set; } = new();

    // Casilla individual (checkbox) para entrega a domicilio.
    [Display(Name = "Entrega a domicilio")]
    public bool EntregaDomicilio { get; set; }

    // Regla de negocio: la dirección es obligatoria solo si hay entrega a domicilio.
    // El atributo personalizado RequeridoSi valida en el servidor y en el cliente.
    [Display(Name = "Dirección de entrega")]
    [RequeridoSi(nameof(EntregaDomicilio), ErrorMessage = "La dirección es obligatoria para entrega a domicilio.")]
    [StringLength(120, ErrorMessage = "La dirección no puede exceder 120 caracteres.")]
    public string? Direccion { get; set; }
}
