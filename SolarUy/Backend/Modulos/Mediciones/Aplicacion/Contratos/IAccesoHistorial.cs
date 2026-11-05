namespace SolarUy.Backend.Modulos.Mediciones.Aplicacion.Contratos;

public interface IAccesoHistorial
{
    Task<bool> PuedeConsultarAsync();
}
