using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using SolarUy.Backend.Modulos.Consultas.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Historial;

namespace SolarUy.Backend.Modulos.Consultas.Aplicacion.Implementaciones;

public static partial class ConsultaNaturalParser
{
    private static readonly string[] ModificacionPalabras =
    [
        "crear", "crea", "creas", "registrar", "modificar", "modifica", "editar",
        "actualizar", "eliminar", "borrar", "cambiar", "cambia", "asignar",
        "activar", "desactivar", "guardar", "enviar", "iniciar", "parar",
        "reiniciar"
    ];

    private static readonly string[] TemaPalabras =
    [
        "parque", "nodo", "panel", "medicion", "mediciones", "generacion",
        "produccion", "historial", "potencia", "datos", "promedio", "diario",
        "crudos", "energia", "muestra", "muestras", "ultimas", "ultimos",
        "dias", "periodo", "hoy", "estado", "orientacion", "orientación",
        "intermedio", "rotura", "bueno", "malo", "mal", "pendiente", "tarea",
        "tareas", "asignada", "asignadas", "pdf", "descarga", "descargar",
        "exportar", "exporta", "archivo", "documento"
    ];

    public static ConsultaNaturalRequest Parsear(string pregunta, IReadOnlyList<OpcionHistorialDto>? opciones = null)
    {
        if (string.IsNullOrWhiteSpace(pregunta))
            throw new ArgumentException("Escribí una pregunta sobre el historial de mediciones.");

        var texto = pregunta.Trim();
        if (texto.Length > 1000)
            throw new ArgumentException("La pregunta supera el máximo de 1000 caracteres.");

        var normalizado = Normalizar(texto);
        if (ModificacionPalabras.Any(palabra => normalizado.Contains(palabra, StringComparison.Ordinal)))
            throw new ArgumentException("La consulta solo consulta información histórica; no se permiten pedidos de modificación.");

        if (!TemaPalabras.Any(palabra => normalizado.Contains(palabra, StringComparison.Ordinal)))
            throw new ArgumentException("La consulta queda fuera de tema para SolarUy.");

        var parqueId = BuscarId(normalizado, "parque") ?? BuscarIdPorNombre(opciones, normalizado, "parque");
        var nodoId = BuscarId(normalizado, "nodo") ?? BuscarIdPorNombre(opciones, normalizado, "nodo", parqueId) ?? ResolverNodoDesdePanel(opciones, BuscarId(normalizado, "panel"));
        var panelId = BuscarId(normalizado, "panel") ?? ResolverPanelDesdeNodo(opciones, nodoId) ?? ResolverPanelPorNombre(opciones, normalizado, parqueId);
        var periodo = DeterminarPeriodo(normalizado);
        var modo = DeterminarModo(normalizado);
        var esListado = EsListado(normalizado);
        if (!parqueId.HasValue)
            parqueId = ResolverParqueDesdeSeleccion(opciones, nodoId, panelId);

        return new ConsultaNaturalRequest(texto, parqueId, nodoId, panelId, periodo, modo, esListado);
    }

    private static string Normalizar(string valor)
    {
        var normalizado = valor.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var caracter in normalizado)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(caracter);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                builder.Append(char.ToLowerInvariant(caracter));
        }

        return builder.ToString();
    }

    private static int? BuscarId(string texto, string palabraClave)
    {
        var match = Regex.Match(texto, $@"(?:^|\s|#){palabraClave}\s*#?\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!match.Success)
            return null;

        return int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static int? BuscarIdPorNombre(IReadOnlyList<OpcionHistorialDto>? opciones, string textoNormalizado, string tipo, int? parqueId = null)
    {
        if (opciones is null || opciones.Count == 0)
            return null;

        var candidatos = new List<object>();
        if (tipo == "parque")
        {
            candidatos.AddRange(opciones.Select(o => new { Id = o.ParqueId, Nombre = o.Parque }));
        }
        else if (tipo == "nodo")
        {
            candidatos.AddRange(opciones
                .Where(o => !parqueId.HasValue || o.ParqueId == parqueId.Value)
                .Select(o => new { Id = o.NodoId, Nombre = o.Nodo }));
        }
        else if (tipo == "panel")
        {
            candidatos.AddRange(opciones
                .Where(o => !parqueId.HasValue || o.ParqueId == parqueId.Value)
                .Select(o => new { Id = o.PanelId, Nombre = $"Panel {o.PanelId}" }));
        }

        foreach (var candidato in candidatos)
        {
            var nombre = candidato.GetType().GetProperty("Nombre")?.GetValue(candidato)?.ToString();
            if (string.IsNullOrWhiteSpace(nombre))
                continue;

            if (CoincideConNombre(textoNormalizado, nombre, tipo))
                return (int?)candidato.GetType().GetProperty("Id")?.GetValue(candidato);
        }

        return null;
    }

    private static int? ResolverNodoDesdePanel(IReadOnlyList<OpcionHistorialDto>? opciones, int? panelId)
    {
        if (!panelId.HasValue || opciones is null || opciones.Count == 0)
            return null;

        var matches = opciones
            .Where(o => o.PanelId == panelId.Value)
            .Select(o => o.NodoId)
            .Distinct()
            .ToArray();

        return matches.Length == 1 ? matches[0] : null;
    }

    private static int? ResolverPanelDesdeNodo(IReadOnlyList<OpcionHistorialDto>? opciones, int? nodoId)
    {
        if (!nodoId.HasValue || opciones is null || opciones.Count == 0)
            return null;

        var matches = opciones
            .Where(o => o.NodoId == nodoId.Value)
            .Select(o => o.PanelId)
            .Distinct()
            .ToArray();

        return matches.Length == 1 ? matches[0] : null;
    }

    private static int? ResolverPanelPorNombre(IReadOnlyList<OpcionHistorialDto>? opciones, string textoNormalizado, int? parqueId)
    {
        if (opciones is null || opciones.Count == 0)
            return null;

        var matches = opciones
            .Where(o => !parqueId.HasValue || o.ParqueId == parqueId.Value)
            .Where(o => CoincideConNombre(textoNormalizado, $"panel {o.PanelId}", "panel"))
            .Select(o => o.PanelId)
            .Distinct()
            .ToArray();

        return matches.Length == 1 ? matches[0] : null;
    }

    private static int? ResolverParqueDesdeSeleccion(IReadOnlyList<OpcionHistorialDto>? opciones, int? nodoId, int? panelId)
    {
        if (opciones is null || opciones.Count == 0)
            return null;

        if (panelId.HasValue)
        {
            var matches = opciones
                .Where(o => o.PanelId == panelId.Value)
                .Select(o => o.ParqueId)
                .Distinct()
                .ToArray();
            if (matches.Length == 1)
                return matches[0];
        }

        if (nodoId.HasValue)
        {
            var matches = opciones
                .Where(o => o.NodoId == nodoId.Value)
                .Select(o => o.ParqueId)
                .Distinct()
                .ToArray();
            if (matches.Length == 1)
                return matches[0];
        }

        return null;
    }

    private static bool CoincideConNombre(string textoNormalizado, string nombre, string tipo)
    {
        var nombreNormalizado = SanearParaCoincidencia(nombre);
        var textoSanizado = SanearParaCoincidencia(textoNormalizado);

        if (string.IsNullOrWhiteSpace(nombreNormalizado) || string.IsNullOrWhiteSpace(textoSanizado))
            return false;

        return Regex.IsMatch(textoSanizado, $@"\b{Regex.Escape(nombreNormalizado)}\b", RegexOptions.CultureInvariant);
    }

    private static string SanearParaCoincidencia(string valor)
    {
        var normalizado = Normalizar(valor);
        var tokens = Regex.Replace(normalizado, "[^a-z0-9]+", " ", RegexOptions.CultureInvariant)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token => token switch
            {
                "parq" => "parque",
                "parques" => "parque",
                "nod" => "nodo",
                "nodos" => "nodo",
                "pan" => "panel",
                "paneles" => "panel",
                _ => token
            })
            .Where(token => !string.IsNullOrWhiteSpace(token));

        return string.Join(" ", tokens);
    }

    private static PeriodoHistorial DeterminarPeriodo(string texto)
    {
        if (texto.Contains("hoy") || texto.Contains("ultimas 24 horas") || texto.Contains("ultimas 24h") || texto.Contains("24 horas") || texto.Contains("24h"))
            return PeriodoHistorial.Ultimas24Horas;
        if (texto.Contains("ultimos 30 dias") || texto.Contains("ultimos 30 días") || texto.Contains("30 dias") || texto.Contains("30 días") || texto.Contains("mes"))
            return PeriodoHistorial.Ultimos30Dias;
        if (texto.Contains("ultimos 365 dias") || texto.Contains("ultimos 365 días") || texto.Contains("365 dias") || texto.Contains("365 días") || texto.Contains("anio") || texto.Contains("año"))
            return PeriodoHistorial.Ultimos365Dias;
        if (texto.Contains("todos los datos") || texto.Contains("todos") || texto.Contains("todo el historial"))
            return PeriodoHistorial.Todos;

        return PeriodoHistorial.Todos;
    }

    private static ModoHistorial DeterminarModo(string texto)
    {
        if (texto.Contains("crudos") || texto.Contains("datos crudos") || texto.Contains("raw"))
            return ModoHistorial.Crudos;

        if (texto.Contains("promedio") || texto.Contains("media") || texto.Contains("promedios"))
            return ModoHistorial.Promedio;

        if (texto.Contains("diario") || texto.Contains("dia") || texto.Contains("día"))
            return ModoHistorial.Diario;

        return ModoHistorial.Diario;
    }

    private static bool EsListado(string texto)
    {
        if (texto.Contains("cuantos") || texto.Contains("cuántos") || texto.Contains("cual") || texto.Contains("cuál") ||
            texto.Contains("que") || texto.Contains("mas") || texto.Contains("más") || texto.Contains("menos") ||
            texto.Contains("mayor") || texto.Contains("menor") || texto.Contains("estado") || texto.Contains("orientacion") ||
            texto.Contains("orientación") || texto.Contains("intermedio") || texto.Contains("rotura") || texto.Contains("bueno") ||
            texto.Contains("malo") || texto.Contains("mal") || texto.Contains("produ") || texto.Contains("genera") ||
            texto.Contains("promedio") || texto.Contains("media"))
            return false;

        var contieneListadoExplicito = texto.Contains("lista") || texto.Contains("listar") || texto.Contains("mostrar") || texto.Contains("muestrame") || texto.Contains("muestreme");
        var contieneEntidades = texto.Contains("parques") || texto.Contains("nodos") || texto.Contains("paneles");
        var contieneDameConEntidades = (texto.Contains("dame") || texto.Contains("dame la") || texto.Contains("dame los") || texto.Contains("dame las")) && contieneEntidades;

        return contieneListadoExplicito || contieneEntidades || contieneDameConEntidades;
    }
}
