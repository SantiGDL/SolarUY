namespace SolarUy.Backend.Modulos.Parques.Dominio.Entidades;

public class Parque
{
    private Parque()
    {
    }

    public int Id { get; private set; }

    public string Nombre { get; private set; } = string.Empty;

    public string Direccion { get; private set; } = string.Empty;

    public string NombreNormalizado { get; private set; } = string.Empty;

    public static Parque Crear(string nombre, string direccion)
    {
        var nombreOriginal = nombre?.Trim() ?? string.Empty;
        var nombreNormalizado = NormalizarNombre(nombreOriginal);
        var direccionNormalizada = direccion?.Trim() ?? string.Empty;

        if (nombreNormalizado.Length is 0 or > 100)
        {
            throw new ArgumentException(
                "El nombre es obligatorio y debe tener como máximo 100 caracteres.",
                nameof(nombre));
        }

        if (direccionNormalizada.Length is 0 or > 250)
        {
            throw new ArgumentException(
                "La dirección es obligatoria y debe tener como máximo 250 caracteres.",
                nameof(direccion));
        }

        return new Parque
        {
            Nombre = nombreOriginal,
            Direccion = direccionNormalizada,
            NombreNormalizado = nombreNormalizado
        };
    }

    public static string NormalizarNombre(string nombre) =>
        (nombre ?? string.Empty).Trim().ToUpperInvariant();
}
