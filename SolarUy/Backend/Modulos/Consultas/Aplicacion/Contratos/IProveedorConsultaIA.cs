namespace SolarUy.Backend.Modulos.Consultas.Aplicacion.Contratos;

public interface IProveedorConsultaIA
{
    Task<string> ObtenerRespuestaAsync(string pregunta, CancellationToken cancellationToken = default);
}
