namespace SolarUy.Backend.Modulos.Parques.Aplicacion.Excepciones;

public sealed class PanelesPersistenciaException(string message, Exception? innerException = null)
    : Exception(message, innerException);
