namespace SolarUy.Backend.Modulos.Tareas.Aplicacion.Excepciones;

public sealed class TareaAutomaticaException(string message, Exception innerException)
    : Exception(message, innerException);
