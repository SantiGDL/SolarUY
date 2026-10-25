using Microsoft.AspNetCore.Components.Authorization;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;
namespace SolarUy.Components.Seguridad;

public sealed class UsuarioActual(AuthenticationStateProvider autenticacion) : IUsuarioActual
{
    public async Task<bool> EstaAutenticadoAsync() =>
        (await autenticacion.GetAuthenticationStateAsync()).User.Identity?.IsAuthenticated == true;
    public async Task<bool> EsAdministradorAsync() =>
        (await autenticacion.GetAuthenticationStateAsync()).User.IsInRole("ADMIN");
}
