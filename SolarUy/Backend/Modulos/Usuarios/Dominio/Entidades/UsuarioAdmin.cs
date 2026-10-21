namespace SolarUy.Backend.Modulos.Usuarios.Dominio.Entidades;

public class UsuarioAdmin : Usuario
{
    public string Correo { get; set; } = string.Empty;

    public string Pass { get; set; } = string.Empty;

    public string Imagen { get; set; } = string.Empty;
}

