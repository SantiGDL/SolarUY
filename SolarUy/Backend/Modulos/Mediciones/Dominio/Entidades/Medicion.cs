namespace SolarUy.Backend.Modulos.Mediciones.Dominio.Entidades;

public sealed class Medicion
{
    private Medicion() { }
    public int Id { get; private set; }
    public int ParqueId { get; private set; }
    public int NodoId { get; private set; }
    public int PanelId { get; private set; }
    public double ValorGenerado { get; private set; }
    public DateTimeOffset FechaHora { get; private set; }

    public static Medicion Crear(int parqueId, int nodoId, int panelId,
        double valorGenerado, DateTimeOffset fechaHora)
    {
        if (parqueId <= 0 || nodoId <= 0 || panelId <= 0)
            throw new ArgumentException("Parque, nodo y panel deben ser válidos.");
        if (!double.IsFinite(valorGenerado) || valorGenerado < 0)
            throw new ArgumentOutOfRangeException(nameof(valorGenerado), "La generación debe ser finita y no negativa.");
        if (fechaHora == default)
            throw new ArgumentException("La fecha de medición es obligatoria.", nameof(fechaHora));
        return new Medicion { ParqueId = parqueId, NodoId = nodoId, PanelId = panelId,
            ValorGenerado = valorGenerado, FechaHora = fechaHora.ToUniversalTime() };
    }
}
