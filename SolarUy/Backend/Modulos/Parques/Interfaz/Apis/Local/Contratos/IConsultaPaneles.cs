namespace SolarUy.Backend.Modulos.Parques.Interfaz.Apis.Local.Contratos;

public interface IConsultaPaneles
{
    Task<bool> ExisteAsync(int panelId, CancellationToken cancellationToken = default);
}
