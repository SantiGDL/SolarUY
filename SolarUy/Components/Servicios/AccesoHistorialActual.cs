using Microsoft.AspNetCore.Components.Authorization;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.Contratos;

namespace SolarUy.Components.Servicios;

public sealed class AccesoHistorialActual(AuthenticationStateProvider autenticacion) : IAccesoHistorial
{
    public async Task<bool> PuedeConsultarAsync()
    {
        var estado = await autenticacion.GetAuthenticationStateAsync();
        return estado.User.Identity?.IsAuthenticated == true && estado.User.IsInRole("ADMIN");
    }
}
