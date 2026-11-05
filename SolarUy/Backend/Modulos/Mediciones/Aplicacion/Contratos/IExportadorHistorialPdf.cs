using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;

namespace SolarUy.Backend.Modulos.Mediciones.Aplicacion.Contratos;

public interface IExportadorHistorialPdf
{
    byte[] Generar(ResultadoHistorialDto resultado);
}
