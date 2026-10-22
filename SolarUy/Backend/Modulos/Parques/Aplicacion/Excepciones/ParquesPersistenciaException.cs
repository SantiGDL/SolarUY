namespace SolarUy.Backend.Modulos.Parques.Aplicacion.Excepciones;

public sealed class ParquesPersistenciaException(string message, Exception innerException)
    : Exception(message, innerException);
