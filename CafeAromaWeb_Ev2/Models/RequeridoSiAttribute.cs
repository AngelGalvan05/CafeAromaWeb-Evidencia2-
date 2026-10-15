using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CafeAromaWeb.Models;

/// <summary>
/// Validación personalizada: el campo es obligatorio solo si otra propiedad booleana
/// (por ejemplo, <c>EntregaDomicilio</c>) vale <c>true</c>.
/// Funciona en el servidor (IsValid) y en el cliente (AddValidation genera atributos data-val-*
/// que lee el archivo wwwroot/js/pedido.js).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class RequeridoSiAttribute : ValidationAttribute, IClientModelValidator
{
    /// <summary>Nombre de la propiedad booleana que activa la regla.</summary>
    public string PropiedadCondicion { get; }

    public RequeridoSiAttribute(string propiedadCondicion)
    {
        PropiedadCondicion = propiedadCondicion;
    }

    // ---- Validación en el SERVIDOR ----
    protected override ValidationResult? IsValid(object? valor, ValidationContext contexto)
    {
        var propiedad = contexto.ObjectType.GetProperty(PropiedadCondicion);
        bool condicionActiva = propiedad?.GetValue(contexto.ObjectInstance) is true;

        if (condicionActiva && string.IsNullOrWhiteSpace(valor as string))
        {
            var miembros = contexto.MemberName is null ? null : new[] { contexto.MemberName };
            return new ValidationResult(ErrorMessage ?? "Este campo es obligatorio.", miembros);
        }
        return ValidationResult.Success;
    }

    // ---- Validación en el CLIENTE ----
    // Agrega al <input> los atributos que jQuery Validation Unobtrusive necesita:
    //   data-val="true"
    //   data-val-requeridosi="mensaje de error"
    //   data-val-requeridosi-propiedad="EntregaDomicilio"
    public void AddValidation(ClientModelValidationContext contexto)
    {
        contexto.Attributes.TryAdd("data-val", "true");
        contexto.Attributes.TryAdd("data-val-requeridosi", ErrorMessage ?? "Este campo es obligatorio.");
        contexto.Attributes.TryAdd("data-val-requeridosi-propiedad", PropiedadCondicion);
    }
}
