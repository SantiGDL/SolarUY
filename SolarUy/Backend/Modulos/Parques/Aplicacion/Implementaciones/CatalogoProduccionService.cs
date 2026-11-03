using SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Excepciones;
using SolarUy.Backend.Modulos.Parques.Dominio.Repositorios;
using SolarUy.Backend.Modulos.Parques.Interfaz.Apis.Local.Contratos;

namespace SolarUy.Backend.Modulos.Parques.Aplicacion.Implementaciones;

public sealed class CatalogoProduccionService(IParqueRepositorio parques,
    INodoRepositorio nodos, IPanelRepositorio paneles) : ICatalogoProduccion
{
    public async Task<IReadOnlyList<ParqueProduccion>> ObtenerAsync(bool esAdministrador,
        CancellationToken cancellationToken)
    {
        if (!esAdministrador)
            throw new AccesoNoAutorizadoException("El simulador requiere un administrador autenticado.");
        var listaParques = await parques.ListarAsync(cancellationToken);
        var listaNodos = await nodos.ListarTodosAsync(cancellationToken);
        var listaPaneles = await paneles.ListarAsync(cancellationToken);
        return listaParques.OrderBy(p => p.Id).Select(p => new ParqueProduccion(p.Id, p.Nombre,
            listaNodos.Where(n => n.ParqueId == p.Id).OrderBy(n => n.Id)
                .Select(n => new NodoProduccion(n.Id, n.Nombre, n.UbicacionDentroParque,
                    listaPaneles.Where(panel => panel.NodoId == n.Id).OrderBy(panel => panel.Id)
                        .Select(panel => new PanelProduccion(panel.Id, panel.PotenciaActual,
                            panel.Orientacion, panel.Estado.ToString())).ToArray()))
                .Where(n => n.Paneles.Count > 0).ToArray()))
            .Where(p => p.Nodos.Count > 0).ToArray();
    }
}
