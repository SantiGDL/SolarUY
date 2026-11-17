namespace SolarUy.Backend.Modulos.Tareas.Aplicacion.Contratos;

public interface IAccesoTareas
{
    Task<bool> EsAdministradorAsync();
}
