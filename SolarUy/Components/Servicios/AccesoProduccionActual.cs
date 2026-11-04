using System.Security.Claims;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;

namespace SolarUy.Components.Servicios;

public static class AccesoProduccionActual
{
    public static AccesoProduccion Desde(ClaimsPrincipal usuario) => new(
        usuario.FindFirstValue("sub") ?? usuario.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
        usuario.Identity?.IsAuthenticated == true && usuario.IsInRole("ADMIN"));
}
