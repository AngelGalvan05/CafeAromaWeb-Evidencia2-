// =============================================================
//  CafeAromaWeb - Punto de entrada de la aplicación
//  Configura los servicios de Razor Pages y la canalización HTTP.
// =============================================================
using System.Globalization;

// Cultura de México: los precios se muestran como $35.00 y las fechas en español.
var culturaMx = new CultureInfo("es-MX");
CultureInfo.DefaultThreadCurrentCulture = culturaMx;
CultureInfo.DefaultThreadCurrentUICulture = culturaMx;

var builder = WebApplication.CreateBuilder(args);

// Registra Razor Pages y traduce al español los mensajes de enlace de modelo
// que se generan automáticamente (por ejemplo, cuando se escribe texto en un campo numérico).
builder.Services.AddRazorPages()
    .AddMvcOptions(opciones =>
    {
        var mensajes = opciones.ModelBindingMessageProvider;
        mensajes.SetValueMustBeANumberAccessor(campo => $"El campo {campo} debe ser un número.");
        mensajes.SetValueMustNotBeNullAccessor(valor => "Este campo es obligatorio.");
        mensajes.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"El valor '{valor}' no es válido para {campo}.");
        mensajes.SetMissingBindRequiredValueAccessor(campo => $"Falta el valor de {campo}.");
    });

var app = builder.Build();

// En producción se usa una página de error amigable y HSTS.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
