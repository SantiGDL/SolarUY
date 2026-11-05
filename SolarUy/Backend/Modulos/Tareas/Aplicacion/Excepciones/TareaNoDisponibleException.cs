namespace SolarUy.Backend.Modulos.Tareas.Aplicacion.Excepciones;

public sealed class TareaNoDisponibleException(string message) : Exception(message);
