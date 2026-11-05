namespace SolarUy.Backend.Modulos.Tareas.Aplicacion.Excepciones;

public sealed class TareasPersistenciaException(string message, Exception innerException)
    : Exception(message, innerException);
