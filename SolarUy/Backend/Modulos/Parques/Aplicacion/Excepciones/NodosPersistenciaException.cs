namespace SolarUy.Backend.Modulos.Parques.Aplicacion.Excepciones;

public sealed class NodosPersistenciaException(string message, Exception? innerException = null)
    : Exception(message, innerException);
