namespace SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;
public interface IUsuarioActual
{
    Task<bool> EstaAutenticadoAsync();
    Task<bool> EsAdministradorAsync();
}
