using System.Collections.Concurrent;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;

namespace SolarUy.Backend.Modulos.Mediciones.Infraestructura.Sesiones;

public sealed class SesionesProduccion : ISesionesProduccion
{
    private readonly ConcurrentDictionary<Guid, SesionProduccion> _sesiones = new();
    private readonly ConcurrentDictionary<int, SemaphoreSlim> _nodos = new();
    public SesionProduccion Crear(string propietario)
    {
        var sesion = new SesionProduccion(Guid.NewGuid(), propietario);
        _sesiones[sesion.Id] = sesion;
        return sesion;
    }
    public SesionProduccion Obtener(Guid id) => _sesiones.TryGetValue(id, out var sesion)
        ? sesion : throw new ArgumentException("La sesión de Producción ya no está disponible. Volvé a abrir la página.");
    public IReadOnlyList<SesionProduccion> Listar() => _sesiones.Values.ToArray();
    public SemaphoreSlim BloqueoNodo(int nodoId) => _nodos.GetOrAdd(nodoId, _ => new SemaphoreSlim(1, 1));
    public void Retirar(Guid id) => _sesiones.TryRemove(id, out _);
}
