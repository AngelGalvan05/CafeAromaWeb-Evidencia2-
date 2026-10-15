// ===========================================================
//  CafeAromaWeb - pedido.js
//  Lógica del CLIENTE para el formulario de negocio (Pages/Pedido.cshtml):
//    1. Regla de validación personalizada "requeridosi" para jQuery Validation.
//    2. Cálculo del total estimado en tiempo real.
//    3. Mostrar/ocultar la dirección según la casilla de entrega.
//    4. Limpieza de datos al escribir (teléfono y nombre).
//    5. Confirmación con cuadro de diálogo antes de enviar.
//  Requiere: jQuery, jquery.validate, jquery.validate.unobtrusive y site.js.
// ===========================================================

// ---------- 0. Mensajes predeterminados de jQuery Validation en español ----------
// Se usan solo cuando una regla no trae su propio mensaje desde el modelo de C#.
$.extend($.validator.messages, {
    required: "Este campo es obligatorio.",
    email: "Escribe un correo electrónico válido.",
    number: "Escribe un número válido.",
    digits: "Escribe solo dígitos.",
    maxlength: $.validator.format("No escribas más de {0} caracteres."),
    minlength: $.validator.format("Escribe al menos {0} caracteres."),
    range: $.validator.format("Escribe un valor entre {0} y {1}."),
    min: $.validator.format("Escribe un valor mayor o igual a {0}."),
    max: $.validator.format("Escribe un valor menor o igual a {0}.")
});

// ---------- 1. Validación personalizada en el cliente ----------
// El atributo C# [RequeridoSi] genera en el <input> de dirección:
//   data-val-requeridosi="mensaje"   data-val-requeridosi-propiedad="EntregaDomicilio"
// Aquí se registra el método que evalúa esa regla en el navegador.

$.validator.addMethod("requeridosi", function (valor, elemento, parametros) {
    const casilla = document.getElementById(parametros.idCasilla);
    // Si la casilla está marcada, el campo no puede quedar vacío.
    if (casilla && casilla.checked) {
        return valor.trim().length > 0;
    }
    return true;
});

// El adaptador traduce los atributos data-val-* en una regla de jQuery Validation.
$.validator.unobtrusive.adapters.add("requeridosi", ["propiedad"], function (opciones) {
    // El id del campo es "Datos_Direccion"; el prefijo "Datos_" se reutiliza para
    // encontrar la casilla: "Datos_EntregaDomicilio".
    const idCampo = opciones.element.id;
    const prefijo = idCampo.substring(0, idCampo.lastIndexOf("_") + 1);
    opciones.rules["requeridosi"] = { idCasilla: prefijo + opciones.params.propiedad };
    opciones.messages["requeridosi"] = opciones.message;
});

// ---------- 2 a 5. Comportamiento del formulario ----------
document.addEventListener("DOMContentLoaded", function () {
    const formulario = document.getElementById("frmPedido");
    if (!formulario) {
        return; // Se está mostrando el resumen del pedido; no hay formulario.
    }

    // Referencias a los elementos de la interfaz (DOM).
    const selProducto = document.getElementById("Datos_ProductoId");
    const txtCantidad = document.getElementById("Datos_Cantidad");
    const txtTelefono = document.getElementById("Datos_Telefono");
    const txtNombre = document.getElementById("Datos_Nombre");
    const chkEntrega = document.getElementById("Datos_EntregaDomicilio");
    const txtDireccion = document.getElementById("Datos_Direccion");
    const grupoDireccion = document.getElementById("grupoDireccion");
    const lblTotal = document.getElementById("lblTotal");
    const lblDetalle = document.getElementById("lblDetalle");

    /**
     * Lee los valores actuales del formulario y los devuelve en un objeto.
     * @returns {{productoId: string, cantidad: number, recargoTamano: number|null,
     *            tamano: string, extras: number, precioExtra: number, envio: number}}
     */
    function leerDatosFormulario() {
        const radioTamano = formulario.querySelector("input[name='Datos.Tamano']:checked");
        const extrasMarcados = formulario.querySelectorAll(".chk-extra:checked");

        return {
            productoId: selProducto.value,
            cantidad: parseInt(txtCantidad.value, 10),
            tamano: radioTamano ? radioTamano.value : "",
            recargoTamano: radioTamano ? parseFloat(radioTamano.dataset.recargo) : null,
            extras: extrasMarcados.length,
            precioExtra: extrasMarcados.length > 0 ? parseFloat(extrasMarcados[0].dataset.precio) : 0,
            envio: chkEntrega.checked ? parseFloat(chkEntrega.dataset.precio) : 0
        };
    }

    /**
     * Calcula el total con la MISMA fórmula que CalcularTotal() del servidor:
     * (precio + recargo de tamaño + extras × precioExtra) × cantidad + envío.
     * @returns {number|null} Total, o null si faltan datos para calcularlo.
     */
    function calcularTotalCliente(datos) {
        const precio = preciosProductos[datos.productoId];
        if (precio === undefined || datos.recargoTamano === null) {
            return null;
        }
        const cantidad = isNaN(datos.cantidad) || datos.cantidad < 1 ? 1 : datos.cantidad;
        const precioUnidad = precio + datos.recargoTamano + datos.extras * datos.precioExtra;
        return precioUnidad * cantidad + datos.envio;
    }

    /** Escribe el total estimado y el desglose en el panel lateral. */
    function actualizarTotal() {
        const datos = leerDatosFormulario();
        const total = calcularTotalCliente(datos);

        if (total === null) {
            lblTotal.textContent = formatearMoneda(0);
            lblDetalle.textContent = "Elige un producto y un tamaño.";
            return;
        }

        const cantidad = isNaN(datos.cantidad) || datos.cantidad < 1 ? 1 : datos.cantidad;
        let detalle = cantidad + " pieza(s), tamaño " + datos.tamano.toLowerCase();
        if (datos.extras > 0) {
            detalle += ", " + datos.extras + (datos.extras === 1 ? " extra" : " extras");
        }
        detalle += datos.envio > 0 ? ", con envío" : ", recoger en tienda";

        lblTotal.textContent = formatearMoneda(total);
        lblDetalle.textContent = detalle + ".";
    }

    /**
     * Muestra el campo de dirección solo cuando se marca la entrega a domicilio.
     * @param {boolean} enfocar - true para colocar el cursor en la dirección al mostrarla.
     */
    function alternarDireccion(enfocar) {
        const mostrar = chkEntrega.checked;
        grupoDireccion.classList.toggle("d-none", !mostrar);
        if (mostrar) {
            if (enfocar) txtDireccion.focus();
        } else {
            txtDireccion.value = "";
            // Quita el mensaje de error que pudiera haber quedado visible.
            $(txtDireccion).removeClass("input-validation-error");
            $("[data-valmsg-for='Datos.Direccion']").text("");
        }
    }

    // ----- Manejo de eventos de la interfaz -----

    // Cualquier cambio en el formulario recalcula el total (delegación de eventos).
    formulario.addEventListener("input", actualizarTotal);
    formulario.addEventListener("change", actualizarTotal);

    chkEntrega.addEventListener("change", function () { alternarDireccion(true); });

    // Teléfono: elimina en vivo todo lo que no sea dígito (expresión regular).
    txtTelefono.addEventListener("input", function () {
        this.value = this.value.replace(/\D/g, "").substring(0, 10);
    });

    // Nombre: al salir del campo se normalizan mayúsculas y espacios (función de site.js).
    txtNombre.addEventListener("blur", function () {
        this.value = normalizarNombre(this.value);
    });

    // Botón Limpiar: pide confirmación antes de borrar lo capturado.
    document.getElementById("btnLimpiar").addEventListener("click", function () {
        if (confirm("¿Deseas borrar los datos capturados?")) {
            formulario.reset();
            $(formulario).validate().resetForm();      // borra los mensajes de validación
            formulario.querySelectorAll(".input-validation-error")
                .forEach(el => el.classList.remove("input-validation-error"));
            alternarDireccion(false);
            actualizarTotal();
        }
    });

    // Antes de enviar: si los datos son válidos, se pide confirmación con el total.
    formulario.addEventListener("submit", function (evento) {
        if (!$(formulario).valid()) {
            console.warn("Formulario con errores; no se envía.");
            return; // jQuery Validation muestra los mensajes y detiene el envío.
        }
        const total = calcularTotalCliente(leerDatosFormulario());
        const nombreProducto = selProducto.options[selProducto.selectedIndex].text.split(" - ")[0];
        const mensaje = "¿Confirmas tu pedido de " + nombreProducto + " por " + formatearMoneda(total) + "?";

        if (!confirm(mensaje)) {
            evento.preventDefault(); // el usuario canceló
        }
    });

    // Estado inicial (también cuando el servidor devuelve la página con errores).
    alternarDireccion(false);
    actualizarTotal();
});
