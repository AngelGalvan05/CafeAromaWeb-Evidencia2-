// ===========================================================
//  CafeAromaWeb - Funciones de JavaScript comunes a todo el sitio
//  Se cargan desde _Layout.cshtml, por lo que están disponibles
//  en las cinco páginas.
// ===========================================================

/**
 * Da formato de moneda mexicana a un número.
 * Función definida por el usuario que usa la función predefinida Intl.NumberFormat.
 * @param {number} cantidad - Importe a formatear.
 * @returns {string} Texto como "$1,250.00".
 */
function formatearMoneda(cantidad) {
    return new Intl.NumberFormat("es-MX", { style: "currency", currency: "MXN" }).format(cantidad);
}

/**
 * Quita espacios sobrantes y pone en mayúscula la primera letra de cada palabra.
 * Ejemplo: "  ana   lópez " -> "Ana López". Usa métodos del objeto String.
 * @param {string} texto - Texto capturado por el usuario.
 * @returns {string} Texto normalizado.
 */
function normalizarNombre(texto) {
    return texto
        .trim()                       // elimina espacios al inicio y al final
        .split(/\s+/)                 // separa por uno o más espacios
        .filter(palabra => palabra.length > 0)
        .map(palabra => palabra.charAt(0).toUpperCase() + palabra.substring(1).toLowerCase())
        .join(" ");
}

// Marca como activo el enlace del menú que corresponde a la página actual (acceso al DOM).
document.addEventListener("DOMContentLoaded", function () {
    const rutaActual = window.location.pathname.toLowerCase();
    const enlaces = document.querySelectorAll(".navbar-nav .nav-link");

    enlaces.forEach(function (enlace) {
        const destino = enlace.getAttribute("href").toLowerCase();
        const esInicio = destino === "/" && (rutaActual === "/" || rutaActual === "/index");
        if (esInicio || (destino !== "/" && rutaActual.startsWith(destino))) {
            enlace.classList.add("active", "fw-semibold");
            enlace.setAttribute("aria-current", "page");
        }
    });

    // Mensaje para quien revise la consola en las herramientas de desarrollador (F12).
    console.log("CafeAromaWeb cargado:", rutaActual);
});
