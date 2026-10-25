namespace SolarUy.Backend.Modulos.Parques.Aplicacion.Excepciones;
public sealed class ParquesPersistenciaException(string mensaje, Exception causa) : Exception(mensaje, causa);
