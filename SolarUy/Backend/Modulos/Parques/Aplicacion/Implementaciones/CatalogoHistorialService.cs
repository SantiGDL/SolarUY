using SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Parques.Dominio.Repositorios;
using SolarUy.Backend.Modulos.Parques.Interfaz.Apis.Local.Contratos;

namespace SolarUy.Backend.Modulos.Parques.Aplicacion.Implementaciones;

public sealed class CatalogoHistorialService(IParqueRepositorio parques,
    INodoRepositorio nodos, IPanelRepositorio paneles) : ICatalogoHistorial
{
    public async Task<CatalogoHistorial> ObtenerAsync(CancellationToken cancellationToken)
    {
        var listaParques = await parques.ListarAsync(cancellationToken);
        var listaNodos = await nodos.ListarTodosAsync(cancellationToken);
        var listaPaneles = await paneles.ListarAsync(cancellationToken);
        return new(listaParques.Select(p => new ParqueHistorial(p.Id, p.Nombre)).ToArray(),
            listaNodos.Select(n => new NodoHistorial(n.Id, n.ParqueId, n.Nombre)).ToArray(),
            listaPaneles.Select(p => new PanelHistorial(p.Id, p.NodoId)).ToArray());
    }
}
