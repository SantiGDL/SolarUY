using System.Text;
using SolarUy.Backend.Modulos.Consultas.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Consultas.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.Implementaciones;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Historial;
using SolarUy.Backend.Modulos.Parques.Interfaz.Apis.Local.Contratos;
using SolarUy.Backend.Modulos.Tareas.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Tareas.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Tareas.Dominio.Enums;

namespace SolarUy.Backend.Modulos.Consultas.Aplicacion.Implementaciones;

public sealed class ConsultaNaturalService(
    HistorialMedicionesService historial,
    ICatalogoProduccion catalogoProduccion,
    IProveedorConsultaIA proveedor,
    ITareasService tareasService,
    ILogger<ConsultaNaturalService> logger)
{
    public const int MaximoMensajes = 20;
    public const int MaximoCaracteres = 1000;
    public const int MaximoLlamadasHerramientas = 6;

    public async Task<RespuestaConsultaNaturalDto> ResolverAsync(string pregunta, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pregunta))
            throw new ArgumentException("Escribí una pregunta para consultar el historial.");

        if (pregunta.Length > MaximoCaracteres)
            throw new ArgumentException($"La pregunta supera el máximo de {MaximoCaracteres} caracteres.");

        var opciones = await historial.ObtenerOpcionesAsync(cancellationToken);

        if (EsPreguntaTareas(pregunta))
        {
            var tareas = await tareasService.ObtenerTodasAsync(cancellationToken);
            return new RespuestaConsultaNaturalDto(ResponderTareas(tareas, pregunta), true, tareas.Count > 0);
        }

        if (EsPedidoExportacionPdf(pregunta))
        {
            var solicitudPdf = ConsultaNaturalParser.Parsear(pregunta, opciones);
            if (!solicitudPdf.ParqueId.HasValue || solicitudPdf.ParqueId <= 0)
                throw new ArgumentException("Indicá el parque que querés consultar para descargar el PDF del historial.");

            var queryPdf = await historial.ConsultarAsync(
                solicitudPdf.ParqueId.Value,
                solicitudPdf.NodoId,
                solicitudPdf.PanelId,
                solicitudPdf.Periodo,
                solicitudPdf.Modo,
                cancellationToken);

            if (queryPdf.Filas.Count == 0)
            {
                var mensajeSinDatos = $"No hay mediciones disponibles para {queryPdf.Seleccion} en el período {queryPdf.Periodo}, así que no puedo generar un PDF con resultados.";
                return new RespuestaConsultaNaturalDto(mensajeSinDatos, false, false);
            }

            var pdf = await historial.ExportarAsync(queryPdf);
            var nombrePdf = $"SolarUy-historial-{queryPdf.Consulta.Hasta:yyyyMMdd-HHmmss}.pdf";
            var cuerpo = $"Preparé el PDF para {queryPdf.Seleccion} en {queryPdf.Periodo}.\n\n{FormatearResultado(queryPdf, solicitudPdf)}";
            return new RespuestaConsultaNaturalDto(cuerpo, false, queryPdf.Filas.Count > 0, queryPdf.Filas.Count > 100, Math.Min(queryPdf.Filas.Count, 100), queryPdf.Filas.Count, pdf, nombrePdf);
        }

        var solicitud = ConsultaNaturalParser.Parsear(pregunta, opciones);

        if (solicitud.EsListado)
        {
            var listaContextual = FormatearListaContextualizada(opciones, pregunta);
            return new RespuestaConsultaNaturalDto(listaContextual, true, opciones.Count > 0);
        }

        if (EsPreguntaCatalogo(pregunta))
        {
            var catalogo = await catalogoProduccion.ObtenerAsync(true, cancellationToken);
            return new RespuestaConsultaNaturalDto(ResponderCatalogo(opciones, pregunta, catalogo), true, opciones.Count > 0);
        }

        if (!solicitud.ParqueId.HasValue || solicitud.ParqueId <= 0)
            throw new ArgumentException("Indicá el parque que querés consultar para responder con datos históricos.");

        var query = await historial.ConsultarAsync(
            solicitud.ParqueId.Value,
            solicitud.NodoId,
            solicitud.PanelId,
            solicitud.Periodo,
            solicitud.Modo,
            cancellationToken);

        var texto = FormatearResultado(query, solicitud);
        var respuesta = texto;

        try
        {
            var textoProveedor = await proveedor.ObtenerRespuestaAsync(pregunta, cancellationToken);
            if (!string.IsNullOrWhiteSpace(textoProveedor) && textoProveedor.Length <= 500)
                respuesta = $"{textoProveedor}\n\n{texto}";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "El proveedor IA no está disponible; se usa la respuesta basada en datos del historial.");
        }

        var recorte = query.Filas.Count > 100;
        return new RespuestaConsultaNaturalDto(respuesta, false, query.Filas.Count > 0, recorte, Math.Min(query.Filas.Count, 100), query.Filas.Count);
    }

    public static bool EsPedidoExportacionPdf(string pregunta)
    {
        var texto = NormalizarPregunta(pregunta);
        return texto.Contains("pdf", StringComparison.Ordinal)
            || texto.Contains("descarga", StringComparison.Ordinal)
            || texto.Contains("descargar", StringComparison.Ordinal)
            || texto.Contains("exporta", StringComparison.Ordinal)
            || texto.Contains("exportar", StringComparison.Ordinal)
            || texto.Contains("archivo", StringComparison.Ordinal)
            || texto.Contains("documento", StringComparison.Ordinal);
    }

    public static bool EsPreguntaTareas(string pregunta)
    {
        var texto = pregunta.ToLowerInvariant();
        return texto.Contains("tarea") || texto.Contains("tareas") || texto.Contains("pendiente") ||
               texto.Contains("pendientes") || texto.Contains("completada") || texto.Contains("completadas") ||
               texto.Contains("finalizada") || texto.Contains("finalizadas") || texto.Contains("abierta") ||
               texto.Contains("abiertas");
    }

    private static string NormalizarPregunta(string pregunta)
    {
        var normalizado = pregunta.Normalize(System.Text.NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var caracter in normalizado)
        {
            var categoria = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(caracter);
            if (categoria != System.Globalization.UnicodeCategory.NonSpacingMark)
                builder.Append(char.ToLowerInvariant(caracter));
        }

        return builder.ToString();
    }

    public static string ResponderTareas(IReadOnlyList<TareaDto> tareas, string pregunta)
    {
        if (tareas.Count == 0)
            return "No hay tareas registradas.";

        var texto = pregunta.ToLowerInvariant();
        var panelId = BuscarIdEnPregunta(texto, "panel");

        var tareasFiltradas = tareas
            .Where(tarea => !panelId.HasValue || tarea.PanelId == panelId.Value)
            .ToArray();

        var pendientes = tareasFiltradas.Where(t => t.Estado == EstadoTarea.Pendiente).ToArray();
        var completadas = tareasFiltradas.Where(t => t.Estado == EstadoTarea.Completada).ToArray();

        if (texto.Contains("lista") || texto.Contains("listar") || texto.Contains("detalle") || texto.Contains("detalles") || texto.Contains("tipo") || texto.Contains("qué") || texto.Contains("que") || texto.Contains("ver"))
            return FormatearListadoTareas(tareasFiltradas, texto);

        if (texto.Contains("pendiente") || texto.Contains("pendientes") || texto.Contains("abierta") || texto.Contains("abiertas"))
            return pendientes.Length == 0
                ? "No hay tareas pendientes."
                : $"Hay {pendientes.Length} tareas pendientes: {string.Join(", ", pendientes.Select(t => $"Tarea #{t.Id}"))}.";

        if (texto.Contains("completada") || texto.Contains("completadas") || texto.Contains("finalizada") || texto.Contains("finalizadas"))
            return completadas.Length == 0
                ? "No hay tareas completadas."
                : $"Hay {completadas.Length} tareas completadas: {string.Join(", ", completadas.Select(t => $"Tarea #{t.Id}"))}.";

        if (texto.Contains("cuantas") || texto.Contains("cuántas") || texto.Contains("cantidad") || texto.Contains("total") || texto.Contains("hay"))
            return $"Hay {tareasFiltradas.Length} tareas en total: {pendientes.Length} pendientes y {completadas.Length} completadas.";

        return $"Hay {tareasFiltradas.Length} tareas en total: {pendientes.Length} pendientes y {completadas.Length} completadas.";
    }

    private static string FormatearListadoTareas(IReadOnlyList<TareaDto> tareas, string texto)
    {
        var tareasFiltradas = tareas
            .Where(tarea =>
            {
                if (texto.Contains("pendiente") || texto.Contains("pendientes") || texto.Contains("abierta") || texto.Contains("abiertas"))
                    return tarea.Estado == EstadoTarea.Pendiente;
                if (texto.Contains("completada") || texto.Contains("completadas") || texto.Contains("finalizada") || texto.Contains("finalizadas"))
                    return tarea.Estado == EstadoTarea.Completada;
                return true;
            })
            .OrderBy(tarea => tarea.Estado)
            .ThenByDescending(tarea => tarea.FechaCreada)
            .ToArray();

        if (tareasFiltradas.Length == 0)
            return texto.Contains("pendiente") || texto.Contains("pendientes") || texto.Contains("abierta") || texto.Contains("abiertas")
                ? "No hay tareas pendientes."
                : texto.Contains("completada") || texto.Contains("completadas") || texto.Contains("finalizada") || texto.Contains("finalizadas")
                    ? "No hay tareas completadas."
                    : "No hay tareas para listar.";

        var detalle = string.Join("; ", tareasFiltradas.Select(FormatearDetalleTarea));
        return $"Tareas disponibles: {detalle}.";
    }

    private static string FormatearDetalleTarea(TareaDto tarea)
    {
        var estado = tarea.Estado == EstadoTarea.Completada ? "Completada" : "Pendiente";
        var tipo = tarea.TipoTarea == TipoMantenimiento.Reparacion ? "Reparación" : "Limpieza";
        var observaciones = string.IsNullOrWhiteSpace(tarea.Observaciones) ? "sin observaciones" : tarea.Observaciones;
        var fechaCreada = tarea.FechaCreada.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
        var fechaFinalizado = tarea.FechaFinalizado.HasValue ? $", cerrada {tarea.FechaFinalizado.Value:dd/MM/yyyy}" : string.Empty;

        return $"Tarea #{tarea.Id} · Panel #{tarea.PanelId} · {tipo} · {estado}{fechaFinalizado} · creada {fechaCreada} · {observaciones}";
    }

    public static string ResponderCatalogo(IReadOnlyList<OpcionHistorialDto> opciones, string pregunta,
        IReadOnlyList<ParqueProduccion>? catalogoProduccion = null)
    {
        if (opciones.Count == 0)
            return "No hay parques, nodos ni paneles con datos históricos disponibles en este momento.";

        var texto = pregunta.Trim();
        var normalizado = texto.ToLowerInvariant();

        if (normalizado.Contains("lista") || normalizado.Contains("listar") || normalizado.Contains("mostrar") ||
            normalizado.Contains("muestrame") || normalizado.Contains("muestreme") ||
            ((normalizado.Contains("dame") || normalizado.Contains("dame la") || normalizado.Contains("dame los") || normalizado.Contains("dame las")) &&
             (normalizado.Contains("parques") || normalizado.Contains("nodos") || normalizado.Contains("paneles"))))
        {
            var parqueId = BuscarIdEnPregunta(normalizado, "parque");
            var nodoId = BuscarIdEnPregunta(normalizado, "nodo");

            if (normalizado.Contains("nodo") && !normalizado.Contains("panel"))
                return FormatearListaNodos(opciones, parqueId);

            if (normalizado.Contains("panel"))
                return FormatearListaPaneles(opciones, parqueId, nodoId);
        }

        if ((normalizado.Contains("cuantos") || normalizado.Contains("cuántos") || normalizado.Contains("cuanta") || normalizado.Contains("cuánta")) && normalizado.Contains("parque") && !normalizado.Contains("nodo") && !normalizado.Contains("panel"))
            return $"Hay {opciones.Select(o => o.ParqueId).Distinct().Count()} parques disponibles.";

        if ((normalizado.Contains("cuantos") || normalizado.Contains("cuántos") || normalizado.Contains("cuanta") || normalizado.Contains("cuánta")) && normalizado.Contains("nodo"))
        {
            var parqueId = BuscarIdEnPregunta(normalizado, "parque");
            var nodoId = BuscarIdEnPregunta(normalizado, "nodo");

            if (nodoId.HasValue)
            {
                var cantidad = opciones
                    .Where(o => (!parqueId.HasValue || o.ParqueId == parqueId.Value) && o.NodoId == nodoId.Value)
                    .Select(o => o.PanelId)
                    .Distinct()
                    .Count();

                return nodoId.HasValue
                    ? $"El nodo #{nodoId.Value} tiene {cantidad} paneles."
                    : $"Hay {cantidad} nodos disponibles.";
            }

            var nodos = opciones
                .Where(o => !parqueId.HasValue || o.ParqueId == parqueId.Value)
                .Select(o => o.NodoId)
                .Distinct()
                .Count();

            if (parqueId.HasValue)
                return $"El parque #{parqueId.Value} tiene {nodos} nodos.";

            return $"Hay {nodos} nodos disponibles.";
        }

        if ((normalizado.Contains("cuantos") || normalizado.Contains("cuántos") || normalizado.Contains("cuanta") || normalizado.Contains("cuánta")) && normalizado.Contains("panel"))
        {
            var parqueId = BuscarIdEnPregunta(normalizado, "parque");
            var nodoId = BuscarIdEnPregunta(normalizado, "nodo");

            var paneles = opciones
                .Where(o => (!parqueId.HasValue || o.ParqueId == parqueId.Value) && (!nodoId.HasValue || o.NodoId == nodoId.Value))
                .Select(o => o.PanelId)
                .Distinct()
                .Count();

            if (nodoId.HasValue)
                return $"El nodo #{nodoId.Value} tiene {paneles} paneles.";

            if (parqueId.HasValue)
                return $"El parque #{parqueId.Value} tiene {paneles} paneles.";

            return $"Hay {paneles} paneles disponibles.";
        }

        if (normalizado.Contains("parque") && (normalizado.Contains("mas") || normalizado.Contains("más")) && normalizado.Contains("nodo"))
        {
            var parqueConMasNodos = opciones
                .GroupBy(o => new { o.ParqueId, o.Parque })
                .Select(g => new { Parque = g.First().Parque, Nodos = g.Select(x => x.NodoId).Distinct().Count() })
                .OrderByDescending(x => x.Nodos)
                .First();

            return $"El parque con más nodos es {parqueConMasNodos.Parque} con {parqueConMasNodos.Nodos} nodos.";
        }

        if (normalizado.Contains("nodo") && (normalizado.Contains("mas") || normalizado.Contains("más")) && normalizado.Contains("panel"))
        {
            var nodoConMasPaneles = opciones
                .GroupBy(o => new { o.NodoId, o.Nodo, o.Parque })
                .Select(g => new { Nodo = g.First().Nodo, Parque = g.First().Parque, Paneles = g.Select(x => x.PanelId).Distinct().Count() })
                .OrderByDescending(x => x.Paneles)
                .First();

            return $"El nodo con más paneles es {nodoConMasPaneles.Nodo} del parque {nodoConMasPaneles.Parque} con {nodoConMasPaneles.Paneles} paneles.";
        }

        if (catalogoProduccion is not null && catalogoProduccion.Count > 0)
        {
            if (normalizado.Contains("potencia") || normalizado.Contains("watt") || normalizado.Contains("watts"))
            {
                var parqueId = BuscarIdEnPregunta(normalizado, "parque");
                var nodoId = BuscarIdEnPregunta(normalizado, "nodo");
                var panelId = BuscarIdEnPregunta(normalizado, "panel");

                if (panelId.HasValue)
                    return FormatearPotenciaPanel(catalogoProduccion, panelId.Value, parqueId, nodoId);

                if (nodoId.HasValue)
                    return FormatearPotenciaNodo(catalogoProduccion, nodoId.Value, parqueId);

                if (parqueId.HasValue)
                    return FormatearPotenciaParque(catalogoProduccion, parqueId.Value);

                if (normalizado.Contains("panel") && (normalizado.Contains("mas") || normalizado.Contains("más") || normalizado.Contains("mayor")))
                    return FormatearPanelMasPotencia(catalogoProduccion);

                if (normalizado.Contains("panel") && (normalizado.Contains("cada") || normalizado.Contains("todos") || normalizado.Contains("todas")))
                    return FormatearPotenciaPaneles(catalogoProduccion, parqueId, nodoId);

                if (normalizado.Contains("panel") || normalizado.Contains("nodo") || normalizado.Contains("parque"))
                    return FormatearPotenciaPaneles(catalogoProduccion, parqueId, nodoId);
            }

            if (normalizado.Contains("produ") || normalizado.Contains("genera"))
            {
                var parqueId = BuscarIdEnPregunta(normalizado, "parque");
                var nodoId = BuscarIdEnPregunta(normalizado, "nodo");
                var panelId = BuscarIdEnPregunta(normalizado, "panel");

                if (panelId.HasValue)
                    return FormatearProduccionPanel(catalogoProduccion, panelId.Value, parqueId, nodoId);

                if (nodoId.HasValue)
                    return FormatearProduccionNodo(catalogoProduccion, nodoId.Value, parqueId);

                if (parqueId.HasValue)
                    return FormatearProduccionParque(catalogoProduccion, parqueId.Value);

                if (normalizado.Contains("panel") && (normalizado.Contains("mas") || normalizado.Contains("más") || normalizado.Contains("mayor")))
                    return FormatearPanelMasProductivo(catalogoProduccion);

                if (normalizado.Contains("nodo") && (normalizado.Contains("mas") || normalizado.Contains("más") || normalizado.Contains("mayor")))
                    return FormatearNodoMasProductivo(catalogoProduccion);
            }

            if (normalizado.Contains("orientacion") || normalizado.Contains("orientación"))
                return FormatearOrientaciones(catalogoProduccion, pregunta);

            if (normalizado.Contains("estado") || normalizado.Contains("funcion") || normalizado.Contains("mal") || normalizado.Contains("deshabilit") || normalizado.Contains("habilit") || normalizado.Contains("intermedio") || normalizado.Contains("rotura") || normalizado.Contains("bueno"))
                return FormatearEstados(catalogoProduccion, pregunta);
        }

        return FormatearLista(opciones);
    }

    private static bool EsPreguntaCatalogo(string pregunta)
    {
        var texto = pregunta.ToLowerInvariant();
        return (texto.Contains("cuantos") || texto.Contains("cuántos") || texto.Contains("cual") || texto.Contains("cuál") ||
                texto.Contains("que") || texto.Contains("mas") || texto.Contains("más") || texto.Contains("menos") ||
                texto.Contains("mayor") || texto.Contains("menor") || texto.Contains("orientacion") || texto.Contains("orientación") ||
                texto.Contains("estado") || texto.Contains("funcion") || texto.Contains("deshabilit") || texto.Contains("habilit") ||
                texto.Contains("produ") || texto.Contains("genera") || texto.Contains("potencia") || texto.Contains("promedio") || texto.Contains("media") || texto.Contains("watt") || texto.Contains("watts") ||
                texto.Contains("intermedio") || texto.Contains("rotura") || texto.Contains("bueno"))
            && (texto.Contains("parque") || texto.Contains("nodo") || texto.Contains("panel") || texto.Contains("orientacion") || texto.Contains("orientación") || texto.Contains("estado") || texto.Contains("produ") || texto.Contains("genera") || texto.Contains("potencia") || texto.Contains("promedio") || texto.Contains("media") || texto.Contains("watt") || texto.Contains("watts"));
    }

    private static string FormatearPanelMasProductivo(IReadOnlyList<ParqueProduccion> catalogoProduccion)
    {
        var panel = catalogoProduccion
            .SelectMany(parque => parque.Nodos.SelectMany(nodo => nodo.Paneles.Select(panel => new { Parque = parque.Nombre, Nodo = nodo.Nombre, Panel = panel })))
            .OrderByDescending(x => x.Panel.PotenciaActual)
            .First();

        return $"El panel que más produce es Panel #{panel.Panel.Id} del nodo {panel.Nodo} del parque {panel.Parque} con {panel.Panel.PotenciaActual:N2} W.";
    }

    private static string FormatearNodoMasProductivo(IReadOnlyList<ParqueProduccion> catalogoProduccion)
    {
        var nodo = catalogoProduccion
            .SelectMany(parque => parque.Nodos.Select(nodo => new { Parque = parque.Nombre, Nodo = nodo, Potencia = nodo.Paneles.Sum(panel => panel.PotenciaActual) }))
            .OrderByDescending(x => x.Potencia)
            .First();

        return $"El nodo que más produce es {nodo.Nodo.Nombre} del parque {nodo.Parque} con {nodo.Potencia:N2} W en total.";
    }

    private static string FormatearOrientaciones(IReadOnlyList<ParqueProduccion> catalogoProduccion, string pregunta)
    {
        var textoPregunta = pregunta.ToLowerInvariant();
        var panelId = BuscarIdEnPregunta(textoPregunta, "panel");
        if (panelId.HasValue)
        {
            var panel = catalogoProduccion
                .SelectMany(parque => parque.Nodos)
                .SelectMany(nodo => nodo.Paneles)
                .FirstOrDefault(p => p.Id == panelId.Value);

            if (panel is not null)
            {
                var orientacion = panel.Orientacion;
                return $"El panel #{panel.Id} tiene orientación {orientacion}.";
            }
        }

        var orientaciones = catalogoProduccion
            .SelectMany(parque => parque.Nodos)
            .SelectMany(nodo => nodo.Paneles)
            .GroupBy(panel => panel.Orientacion)
            .Select(grupo => new { Orientacion = grupo.Key, Cantidad = grupo.Count() })
            .OrderByDescending(x => x.Cantidad)
            .ThenBy(x => x.Orientacion)
            .ToArray();

        if (orientaciones.Length == 0)
            return "No hay orientaciones registradas para los paneles disponibles.";

        if (orientaciones.Length == 1)
            return $"La orientación de los paneles es {orientaciones[0].Orientacion} ({orientaciones[0].Cantidad} paneles).";

        var resumen = string.Join(", ", orientaciones.Select(x => $"{x.Orientacion} ({x.Cantidad})"));
        return $"Las orientaciones registradas son: {resumen}.";
    }

    private static string FormatearEstados(IReadOnlyList<ParqueProduccion> catalogoProduccion, string pregunta)
    {
        var paneles = catalogoProduccion
            .SelectMany(parque => parque.Nodos)
            .SelectMany(nodo => nodo.Paneles)
            .ToArray();

        if (paneles.Length == 0)
            return "No hay paneles con información de estado disponible.";

        var preguntaNormalizada = pregunta.ToLowerInvariant();
        var panelIdCoincidente = BuscarIdEnPregunta(preguntaNormalizada, "panel");

        if (panelIdCoincidente.HasValue)
        {
            var panel = paneles.FirstOrDefault(p => p.Id == panelIdCoincidente.Value);
            if (panel is null)
                return $"No encontré el panel #{panelIdCoincidente.Value}.";

            var estado = panel.Deshabilitado ? "Deshabilitado" : panel.Estado;
            return $"El panel #{panel.Id} está en estado {estado}.";
        }

        if (preguntaNormalizada.Contains("no bueno") || preguntaNormalizada.Contains("malo") || preguntaNormalizada.Contains("mal") || preguntaNormalizada.Contains("defectuoso") || preguntaNormalizada.Contains("problema"))
        {
            var malos = paneles.Where(p => !p.Deshabilitado && !string.Equals(p.Estado, "Bueno", StringComparison.OrdinalIgnoreCase)).ToArray();
            if (malos.Length == 0)
                return "No hay paneles en estado no bueno.";

            return $"Los paneles en estado no bueno son: {string.Join(", ", malos.Select(p => $"Panel #{p.Id} ({p.Estado})"))}.";
        }

        if (preguntaNormalizada.Contains("buen") || preguntaNormalizada.Contains("bueno"))
        {
            var buenos = paneles.Where(p => !p.Deshabilitado && string.Equals(p.Estado, "Bueno", StringComparison.OrdinalIgnoreCase)).ToArray();
            if (buenos.Length == 0)
                return "No hay paneles en estado bueno.";

            return $"Los paneles en estado bueno son: {string.Join(", ", buenos.Select(p => $"Panel #{p.Id}"))}.";
        }

        var estadoPedido = preguntaNormalizada.Contains("intermedio") ? "Intermedio" :
            preguntaNormalizada.Contains("rotura") || preguntaNormalizada.Contains("roto") ? "Rotura" :
            preguntaNormalizada.Contains("intermedios") ? "Intermedio" : null;

        if (estadoPedido is not null)
        {
            var coincidentes = paneles.Where(p => !p.Deshabilitado && string.Equals(p.Estado, estadoPedido, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (coincidentes.Length == 0)
                return $"No hay paneles en estado {estadoPedido}.";

            return $"Los paneles en estado {estadoPedido} son: {string.Join(", ", coincidentes.Select(p => $"Panel #{p.Id}"))}.";
        }

        var conProblemas = paneles
            .Where(panel => !string.Equals(panel.Estado, "Bueno", StringComparison.OrdinalIgnoreCase) || panel.Deshabilitado)
            .ToArray();

        if (conProblemas.Length == 0)
            return "Todos los paneles están en estado bueno y habilitados.";

        var resumen = conProblemas
            .GroupBy(panel => new { panel.Estado, panel.Deshabilitado })
            .Select(grupo =>
            {
                var descripcion = grupo.Key.Deshabilitado ? "deshabilitado" : grupo.Key.Estado;
                return $"{descripcion} ({grupo.Count()})";
            })
            .ToArray();

        return $"Hay {conProblemas.Length} paneles en estado no bueno o deshabilitados: {string.Join(", ", resumen)}.";
    }

    private static int? BuscarIdEnPregunta(string texto, string palabraClave)
    {
        var match = System.Text.RegularExpressions.Regex.Match(texto, $@"(?:^|\s|#){palabraClave}\s*#?\s*(\d+)", System.Text.RegularExpressions.RegexOptions.CultureInvariant);
        return match.Success ? int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) : null;
    }

    private static string FormatearListaContextualizada(IReadOnlyList<OpcionHistorialDto> opciones, string pregunta)
    {
        var normalizado = pregunta.ToLowerInvariant();
        var parqueId = BuscarIdEnPregunta(normalizado, "parque");
        var nodoId = BuscarIdEnPregunta(normalizado, "nodo");

        if (normalizado.Contains("nodo") && !normalizado.Contains("panel"))
            return FormatearListaNodos(opciones, parqueId);

        if (normalizado.Contains("panel"))
            return FormatearListaPaneles(opciones, parqueId, nodoId);

        return FormatearLista(opciones);
    }

    private static string FormatearListaNodos(IReadOnlyList<OpcionHistorialDto> opciones, int? parqueId = null)
    {
        var nodos = opciones
            .Where(o => !parqueId.HasValue || o.ParqueId == parqueId.Value)
            .GroupBy(o => new { o.ParqueId, o.NodoId, o.Nodo })
            .OrderBy(g => g.Key.ParqueId)
            .ThenBy(g => g.Key.NodoId)
            .Select(g => $"- Nodo #{g.Key.NodoId}: {g.Key.Nodo} (Parque #{g.Key.ParqueId})")
            .ToArray();

        if (nodos.Length == 0)
            return "No hay nodos disponibles para la selección indicada.";

        return "Nodos disponibles:\n" + string.Join(Environment.NewLine, nodos);
    }

    private static string FormatearListaPaneles(IReadOnlyList<OpcionHistorialDto> opciones, int? parqueId = null, int? nodoId = null)
    {
        var paneles = opciones
            .Where(o => (!parqueId.HasValue || o.ParqueId == parqueId.Value) && (!nodoId.HasValue || o.NodoId == nodoId.Value))
            .GroupBy(o => new { o.ParqueId, o.NodoId, o.Nodo, o.PanelId })
            .OrderBy(g => g.Key.ParqueId)
            .ThenBy(g => g.Key.NodoId)
            .ThenBy(g => g.Key.PanelId)
            .Select(g => $"- Panel #{g.Key.PanelId} (Parque #{g.Key.ParqueId}, Nodo #{g.Key.NodoId}: {g.Key.Nodo})")
            .ToArray();

        if (paneles.Length == 0)
            return "No hay paneles disponibles para la selección indicada.";

        return "Paneles disponibles:\n" + string.Join(Environment.NewLine, paneles);
    }

    private static string FormatearProduccionPanel(IReadOnlyList<ParqueProduccion> catalogoProduccion, int panelId, int? parqueId = null, int? nodoId = null)
    {
        var panel = catalogoProduccion
            .SelectMany(parque => parque.Nodos)
            .SelectMany(nodo => nodo.Paneles)
            .FirstOrDefault(p => p.Id == panelId && (!parqueId.HasValue || catalogoProduccion.First(x => x.Id == parqueId.Value).Nodos.Any(n => n.Paneles.Any(panel => panel.Id == p.Id))) && (!nodoId.HasValue || catalogoProduccion.SelectMany(x => x.Nodos).Any(n => n.Id == nodoId.Value && n.Paneles.Any(panel => panel.Id == p.Id))));

        if (panel is null)
            return $"No encontré información de producción para el panel #{panelId}.";

        var parque = catalogoProduccion
            .FirstOrDefault(p => p.Nodos.Any(n => n.Paneles.Any(panelInfo => panelInfo.Id == panelId)));
        var nodo = parque?.Nodos.FirstOrDefault(n => n.Paneles.Any(p => p.Id == panelId));

        return $"La producción del panel #{panel.Id} es {panel.PotenciaActual:N2} W. Parque #{parque?.Id ?? 0}, Nodo #{nodo?.Id ?? 0}.";
    }

    private static string FormatearPotenciaPanel(IReadOnlyList<ParqueProduccion> catalogoProduccion, int panelId, int? parqueId = null, int? nodoId = null)
    {
        var panel = catalogoProduccion
            .SelectMany(parque => parque.Nodos)
            .SelectMany(nodo => nodo.Paneles)
            .FirstOrDefault(p => p.Id == panelId && (!parqueId.HasValue || catalogoProduccion.Any(parque => parque.Id == parqueId.Value && parque.Nodos.Any(nodo => nodo.Paneles.Any(panel => panel.Id == p.Id)))) && (!nodoId.HasValue || catalogoProduccion.SelectMany(parque => parque.Nodos).Any(nodo => nodo.Id == nodoId.Value && nodo.Paneles.Any(panel => panel.Id == p.Id))));

        if (panel is null)
            return $"No encontré la potencia actual del panel #{panelId}.";

        return $"La potencia actual del panel #{panel.Id} es {panel.PotenciaActual:N2} W.";
    }

    private static string FormatearPotenciaNodo(IReadOnlyList<ParqueProduccion> catalogoProduccion, int nodoId, int? parqueId = null)
    {
        var nodo = catalogoProduccion
            .SelectMany(parque => parque.Nodos)
            .FirstOrDefault(n => n.Id == nodoId && (!parqueId.HasValue || catalogoProduccion.Any(parque => parque.Id == parqueId.Value && parque.Nodos.Any(nodo => nodo.Id == n.Id))));

        if (nodo is null)
            return $"No encontré la potencia del nodo #{nodoId}.";

        var total = nodo.Paneles.Sum(panel => panel.PotenciaActual);
        return $"La potencia actual del nodo #{nodo.Id} ({nodo.Nombre}) es {total:N2} W.";
    }

    private static string FormatearPotenciaParque(IReadOnlyList<ParqueProduccion> catalogoProduccion, int parqueId)
    {
        var parque = catalogoProduccion.FirstOrDefault(parque => parque.Id == parqueId);
        if (parque is null)
            return $"No encontré el parque #{parqueId}.";

        var total = parque.Nodos.Sum(nodo => nodo.Paneles.Sum(panel => panel.PotenciaActual));
        return $"La potencia actual del parque #{parque.Id} ({parque.Nombre}) es {total:N2} W.";
    }

    private static string FormatearPanelMasPotencia(IReadOnlyList<ParqueProduccion> catalogoProduccion)
    {
        var panel = catalogoProduccion
            .SelectMany(parque => parque.Nodos)
            .SelectMany(nodo => nodo.Paneles)
            .OrderByDescending(panel => panel.PotenciaActual)
            .First();

        return $"El panel con mayor potencia es Panel #{panel.Id} con {panel.PotenciaActual:N2} W.";
    }

    private static string FormatearPotenciaPaneles(IReadOnlyList<ParqueProduccion> catalogoProduccion, int? parqueId = null, int? nodoId = null)
    {
        var paneles = catalogoProduccion
            .SelectMany(parque => parque.Nodos)
            .SelectMany(nodo => nodo.Paneles)
            .Where(panel => (!parqueId.HasValue || catalogoProduccion.Any(parque => parque.Id == parqueId.Value && parque.Nodos.Any(nodo => nodo.Paneles.Any(panelInNodo => panelInNodo.Id == panel.Id)))) && (!nodoId.HasValue || catalogoProduccion.SelectMany(parque => parque.Nodos).Any(nodo => nodo.Id == nodoId.Value && nodo.Paneles.Any(panelInNodo => panelInNodo.Id == panel.Id))))
            .OrderBy(panel => panel.Id)
            .ToArray();

        if (paneles.Length == 0)
            return "No hay paneles con potencia actual disponible.";

        var resumen = string.Join(", ", paneles.Select(panel => $"Panel #{panel.Id}: {panel.PotenciaActual:N2} W"));
        return $"La potencia actual de los paneles es: {resumen}.";
    }

    private static string FormatearProduccionNodo(IReadOnlyList<ParqueProduccion> catalogoProduccion, int nodoId, int? parqueId = null)
    {
        var nodo = catalogoProduccion
            .SelectMany(parque => parque.Nodos)
            .FirstOrDefault(n => n.Id == nodoId && (!parqueId.HasValue || catalogoProduccion.FirstOrDefault(p => p.Id == parqueId.Value)?.Nodos.Any(nodo => nodo.Id == n.Id) == true));

        if (nodo is null)
            return $"No encontré información de producción para el nodo #{nodoId}.";

        var total = nodo.Paneles.Sum(panel => panel.PotenciaActual);
        var parque = catalogoProduccion.FirstOrDefault(p => p.Nodos.Any(n => n.Id == nodo.Id));
        return $"La producción del nodo #{nodo.Id} ({nodo.Nombre}) del parque #{parque?.Id ?? 0} es {total:N2} W.";
    }

    private static string FormatearProduccionParque(IReadOnlyList<ParqueProduccion> catalogoProduccion, int parqueId)
    {
        var parque = catalogoProduccion.FirstOrDefault(p => p.Id == parqueId);
        if (parque is null)
            return $"No encontré el parque #{parqueId}.";

        var total = parque.Nodos.Sum(nodo => nodo.Paneles.Sum(panel => panel.PotenciaActual));
        return $"La producción del parque #{parque.Id} ({parque.Nombre}) es {total:N2} W.";
    }

    public async Task<string> ListarOpcionesAsync(CancellationToken cancellationToken = default)
    {
        var opciones = await historial.ObtenerOpcionesAsync(cancellationToken);
        return FormatearLista(opciones);
    }

    public async Task<string> ConsultarPorFiltrosAsync(
        int parqueId,
        int? nodoId,
        int? panelId,
        PeriodoHistorial periodo,
        ModoHistorial modo,
        CancellationToken cancellationToken = default)
    {
        var query = await historial.ConsultarAsync(parqueId, nodoId, panelId, periodo, modo, cancellationToken);
        return FormatearResultado(query, new ConsultaNaturalRequest(
            $"Generación del parque {parqueId}",
            parqueId,
            nodoId,
            panelId,
            periodo,
            modo,
            false));
    }

    private static string FormatearLista(IReadOnlyList<OpcionHistorialDto> opciones)
    {
        if (opciones.Count == 0)
            return "No hay parques, nodos ni paneles con datos históricos disponibles en este momento.";

        var grupos = opciones
            .GroupBy(o => o.ParqueId)
            .OrderBy(g => g.Key)
            .Select(grupo =>
            {
                var nodos = grupo
                    .GroupBy(o => o.NodoId)
                    .OrderBy(g => g.Key)
                    .Select(nodo =>
                    {
                        var paneles = string.Join(", ", nodo.OrderBy(p => p.PanelId).Select(p => $"Panel #{p.PanelId}"));
                        return $"  - Nodo #{nodo.Key}: {nodo.First().Nodo} ({paneles})";
                    });

                return $"Parque #{grupo.Key}: {grupo.First().Parque}\n{string.Join(Environment.NewLine, nodos)}";
            });

        return "Opciones disponibles:\n" + string.Join(Environment.NewLine, grupos);
    }

    private static string FormatearResultado(ResultadoHistorialDto query, ConsultaNaturalRequest solicitud)
    {
        var seleccion = query.Seleccion;
        var periodo = query.Periodo;

        if (query.Filas.Count == 0)
            return $"No hay mediciones disponibles para {seleccion} en el período {periodo}. El historial no reemplaza los datos faltantes por cero.";

        var filas = query.Filas.Take(100).ToArray();
        var total = query.Filas.Count;
        var primer = filas.First();
        var ultimo = filas.Last();
        var resumen = query.Consulta.Modo switch
        {
            ModoHistorial.Promedio => $"El promedio observado fue {filas.Average(f => f.Valor):N2} W por panel.",
            ModoHistorial.Crudos => $"Hay {total} registros crudos con valores entre {filas.Min(f => f.Valor):N2} W y {filas.Max(f => f.Valor):N2} W.",
            _ => $"El valor más reciente es {ultimo.Valor:N2} W y la primera lectura del tramo fue {primer.Valor:N2} W."
        };

        var linea = $"Para {seleccion} en {periodo}, {resumen}";
        if (total > 100)
            linea += " Se recortó la salida a las 100 filas más recientes.";

        return linea;
    }
}
