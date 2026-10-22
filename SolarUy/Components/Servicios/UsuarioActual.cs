using Microsoft.AspNetCore.Components.Authorization;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;

namespace SolarUy.Components.Servicios;

public sealed class UsuarioActual(
    AuthenticationStateProvider proveedorAutenticacion) : IUsuarioActual
{
    public async Task<bool> EstaAutenticadoAsync()
    {
        var estado = await proveedorAutenticacion.GetAuthenticationStateAsync();
        return estado.User.Identity?.IsAuthenticated == true;
    }

    public async Task<bool> EsAdministradorAsync()
    {
        var estado = await proveedorAutenticacion.GetAuthenticationStateAsync();
        return estado.User.Identity?.IsAuthenticated == true
            && estado.User.IsInRole("ADMIN");
    }
}
