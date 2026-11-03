namespace SolarUy.Backend.Modulos.Parques.Dominio.Entidades;

public sealed class Nodo
{
    private Nodo()
    {
    }

    public int Id { get; private set; }

    public int ParqueId { get; private set; }

    public string UbicacionDentroParque { get; private set; } = string.Empty;

    public string Nombre { get; private set; } = string.Empty;

    public static Nodo Crear(int parqueId, string nombre, string ubicacionDentroParque)
    {
        var nombreNormalizado = nombre?.Trim() ?? string.Empty;
        var ubicacionNormalizada = ubicacionDentroParque?.Trim() ?? string.Empty;

        if (parqueId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parqueId),
                "El parque asociado debe ser válido.");
        }

        if (nombreNormalizado.Length is 0 or > 100)
        {
            throw new ArgumentException(
                "El nombre es obligatorio y debe tener como máximo 100 caracteres.",
                nameof(nombre));
        }

        if (ubicacionNormalizada.Length is 0 or > 250)
        {
            throw new ArgumentException(
                "La ubicación dentro del parque es obligatoria y debe tener como máximo 250 caracteres.",
                nameof(ubicacionDentroParque));
        }

        return new Nodo
        {
            ParqueId = parqueId,
            Nombre = nombreNormalizado,
            UbicacionDentroParque = ubicacionNormalizada
        };
    }
}
