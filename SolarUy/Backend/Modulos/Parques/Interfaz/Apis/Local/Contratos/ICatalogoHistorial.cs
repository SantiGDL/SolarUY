namespace SolarUy.Backend.Modulos.Parques.Interfaz.Apis.Local.Contratos;

public sealed record ParqueHistorial(int Id, string Nombre);
public sealed record NodoHistorial(int Id, int ParqueId, string Nombre);
public sealed record PanelHistorial(int Id, int NodoId);
public sealed record CatalogoHistorial(IReadOnlyList<ParqueHistorial> Parques,
    IReadOnlyList<NodoHistorial> Nodos, IReadOnlyList<PanelHistorial> Paneles);

public interface ICatalogoHistorial
{
    Task<CatalogoHistorial> ObtenerAsync(CancellationToken cancellationToken);
}
