using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SolarUy.Backend.Modulos.Consultas.Aplicacion.Implementaciones;

namespace SolarUy.Components.Pages;

public partial class Consultas
{
    [Inject] private ConsultaNaturalService Servicio { get; set; } = default!;
    [Inject] private ILogger<Consultas> Logger { get; set; } = default!;
    [Inject] private IJSRuntime Js { get; set; } = default!;

    private readonly List<ChatMensaje> _mensajes =
    [
        new("asistente", "Puedo ayudarte a consultar el historial de generación y mediciones. Preguntá por un parque, un nodo, un panel o un período.")
    ];

    private string _pregunta = string.Empty;
    private string? _error;
    private bool _enviando;
    private byte[]? _pdfActual;
    private string? _nombrePdf;
    private IJSObjectReference? _moduloPdf;

    private async Task EnviarAsync()
    {
        var pregunta = _pregunta.Trim();
        if (string.IsNullOrWhiteSpace(pregunta))
        {
            _error = "Escribí una pregunta para consultar el historial.";
            return;
        }

        _error = null;
        _enviando = true;
        _mensajes.Add(new("usuario", pregunta));
        _pregunta = string.Empty;

        try
        {
            var respuesta = await Servicio.ResolverAsync(pregunta, CancellationToken.None);
            _mensajes.Add(new("asistente", respuesta.Mensaje));
            _pdfActual = respuesta.Pdf;
            _nombrePdf = respuesta.NombrePdf;
            if (_mensajes.Count > ConsultaNaturalService.MaximoMensajes)
                _mensajes.RemoveRange(0, _mensajes.Count - ConsultaNaturalService.MaximoMensajes);
        }
        catch (Exception ex)
        {
            var mensaje = ex is ArgumentException ? ex.Message : "No pude resolver la consulta. Revisá la pregunta o el servicio de historial.";
            Logger.LogWarning(ex, "Falló la consulta en lenguaje natural.");
            _mensajes.Add(new("asistente", mensaje));
            _error = mensaje;
        }
        finally
        {
            _enviando = false;
        }
    }

    private async Task DescargarPdfAsync()
    {
        if (_pdfActual is null || _nombrePdf is null)
            return;

        _moduloPdf ??= await Js.InvokeAsync<IJSObjectReference>("import", "./js/historial.js");
        using var stream = new MemoryStream(_pdfActual);
        using var referencia = new DotNetStreamReference(stream);
        await _moduloPdf.InvokeVoidAsync("descargarPdf", _nombrePdf, referencia);
    }

    private sealed record ChatMensaje(string Rol, string Texto);
}
