using SolarUy.Backend.Modulos.Parques.Dominio.Enums;

namespace SolarUy.Backend.Modulos.Parques.Dominio.Entidades;

public class Panel
{
    private Panel()
    {
    }

    public int Id { get; set; }

    public EstadoPanel Estado { get; set; }

    public double PotenciaActual { get; set; }

    public string Orientacion { get; set; } = string.Empty;

    public int NodoId { get; set; }

    public static Panel Crear(
        int nodoId,
        EstadoPanel estado,
        double potenciaActual,
        string orientacion)
    {
        var orientacionNormalizada = (orientacion ?? string.Empty).Trim();

        if (nodoId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nodoId),
                "El nodo asociado debe ser válido.");
        }

        if (!Enum.IsDefined(estado))
        {
            throw new ArgumentOutOfRangeException(
                nameof(estado),
                "El estado del panel debe ser un valor válido.");
        }

        if (!double.IsFinite(potenciaActual) || potenciaActual < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(potenciaActual),
                "La potencia actual debe ser un número finito mayor o igual a cero.");
        }

        if (orientacionNormalizada.Length is 0 or > 50)
        {
            throw new ArgumentException(
                "La orientación es obligatoria y debe tener como máximo 50 caracteres.",
                nameof(orientacion));
        }

        return new Panel
        {
            NodoId = nodoId,
            Estado = estado,
            PotenciaActual = potenciaActual,
            Orientacion = orientacionNormalizada
        };
    }

    public void Actualizar(
        int nodoId,
        EstadoPanel estado,
        double potenciaActual,
        string orientacion)
    {
        var panelActualizado = Crear(nodoId, estado, potenciaActual, orientacion);
        NodoId = panelActualizado.NodoId;
        Estado = panelActualizado.Estado;
        PotenciaActual = panelActualizado.PotenciaActual;
        Orientacion = panelActualizado.Orientacion;
    }
}

