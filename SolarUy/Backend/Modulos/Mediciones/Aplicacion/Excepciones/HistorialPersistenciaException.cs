namespace SolarUy.Backend.Modulos.Mediciones.Aplicacion.Excepciones;

public sealed class HistorialPersistenciaException(string mensaje, Exception causa) : Exception(mensaje, causa);
