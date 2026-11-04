namespace SolarUy.Backend.Modulos.Parques.Interfaz.Apis.Local.Contratos;

public sealed record PanelProduccion(int Id, double PotenciaActual, string Orientacion, string Estado, bool Deshabilitado);
public sealed record NodoProduccion(int Id, string Nombre, string Ubicacion, IReadOnlyList<PanelProduccion> Paneles);
public sealed record ParqueProduccion(int Id, string Nombre, IReadOnlyList<NodoProduccion> Nodos);

public interface ICatalogoProduccion
{
    Task<IReadOnlyList<ParqueProduccion>> ObtenerAsync(bool esAdministrador, CancellationToken cancellationToken);
    Task CambiarHabilitacionAsync(bool esAdministrador, int parqueId, int nodoId, int panelId,
        bool deshabilitado, CancellationToken cancellationToken);
}
