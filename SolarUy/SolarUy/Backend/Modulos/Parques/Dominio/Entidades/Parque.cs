namespace SolarUy.Backend.Modulos.Parques.Dominio.Entidades;

public sealed class Parque
{
    public const int LongitudMaximaNombre = 100;
    public const int LongitudMaximaDireccion = 250;
    public int Id { get; private set; }
    public string Nombre { get; private set; } = "";
    public string Direccion { get; private set; } = "";
    public string NombreNormalizado { get; private set; } = "";
    private Parque() { }

    public static Parque Crear(string nombre, string direccion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentException.ThrowIfNullOrWhiteSpace(direccion);
        nombre = nombre.Trim();
        direccion = direccion.Trim();
        if (nombre.Length > LongitudMaximaNombre)
            throw new ArgumentException("El nombre admite hasta 100 caracteres.", nameof(nombre));
        if (direccion.Length > LongitudMaximaDireccion)
            throw new ArgumentException("La dirección admite hasta 250 caracteres.", nameof(direccion));
        return new Parque { Nombre = nombre, Direccion = direccion, NombreNormalizado = nombre.ToUpperInvariant() };
    }
}
