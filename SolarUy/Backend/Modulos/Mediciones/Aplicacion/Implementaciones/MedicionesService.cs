using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Entidades;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Repositorios;
using SolarUy.Backend.Modulos.Parques.Interfaz.Apis.Local.Contratos;

namespace SolarUy.Backend.Modulos.Mediciones.Aplicacion.Implementaciones;

public sealed class MedicionesService(ICatalogoProduccion catalogo, IMedicionRepository repositorio)
{
    public Task<IReadOnlyList<ParqueProduccion>> ObtenerCatalogoAsync(bool esAdministrador,
        CancellationToken cancellationToken) => catalogo.ObtenerAsync(esAdministrador, cancellationToken);

    public async Task<NodoProduccion> ValidarSeleccionAsync(bool esAdministrador, int parqueId,
        int nodoId, int? panelId, CancellationToken cancellationToken)
    {
        var parques = await ObtenerCatalogoAsync(esAdministrador, cancellationToken);
        var nodo = parques.SingleOrDefault(p => p.Id == parqueId)?.Nodos.SingleOrDefault(n => n.Id == nodoId)
            ?? throw new ArgumentException("El nodo ya no está disponible. Actualizá el catálogo.");
        if (panelId.HasValue && !nodo.Paneles.Any(p => p.Id == panelId))
            throw new ArgumentException("El panel no pertenece al nodo seleccionado.");
        return nodo;
    }

    public async Task RegistrarEntregaAsync(bool esAdministrador, EntregaMediciones entrega,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entrega);
        var nodo = await ValidarSeleccionAsync(esAdministrador, entrega.ParqueId, entrega.NodoId, null, cancellationToken);
        if (entrega.Lecturas is null || entrega.Lecturas.Count != nodo.Paneles.Count
            || entrega.Lecturas.Select(l => l.PanelId).Distinct().Count() != nodo.Paneles.Count)
            throw new ArgumentException("La entrega debe contener una lectura por cada panel del nodo. Actualizá el catálogo.");
        var mediciones = new List<Medicion>();
        foreach (var lectura in entrega.Lecturas)
        {
            var panel = nodo.Paneles.SingleOrDefault(p => p.Id == lectura.PanelId);
            if (panel is null || lectura.ParqueId != entrega.ParqueId || lectura.NodoId != entrega.NodoId
                || lectura.ValorGenerado > panel.PotenciaActual)
                throw new ArgumentException("Las mediciones no corresponden al catálogo actual. Actualizá la selección.");
            mediciones.Add(Medicion.Crear(lectura.ParqueId, lectura.NodoId, lectura.PanelId,
                lectura.ValorGenerado, lectura.FechaHora));
        }
        await repositorio.AgregarEntregaAsync(mediciones, cancellationToken);
    }
}
