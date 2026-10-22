namespace SolarUy.Backend.Modulos.Parques.Aplicacion.Excepciones;

public sealed class AccesoNoAutorizadoException(string message) : Exception(message);
