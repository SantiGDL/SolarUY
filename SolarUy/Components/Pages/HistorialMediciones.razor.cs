using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.Implementaciones;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Historial;

namespace SolarUy.Components.Pages;

public partial class HistorialMediciones : IAsyncDisposable
{
    [Inject] private HistorialMedicionesService Servicio { get; set; } = default!;
    [Inject] private IJSRuntime Js { get; set; } = default!;
    [Inject] private ILogger<HistorialMediciones> Logger { get; set; } = default!;
    private readonly CancellationTokenSource _vida = new();
    private IReadOnlyList<OpcionHistorialDto> _opciones = [];
    private ResultadoHistorialDto? _resultado;
    private IJSObjectReference? _modulo;
    private string? _error;
    private bool _cargando, _consultando, _exportando;
    private int _parque, _nodo, _panel, _pagina;
    private PeriodoHistorial _periodo = PeriodoHistorial.Ultimas24Horas;
    private ModoHistorial _modo = ModoHistorial.Diario;
    private bool Ocupado => _cargando || _consultando || _exportando;
    private const int TamanoPagina = 50;
    private int Paginas => Math.Max(1, ((_resultado?.Filas.Count ?? 0) + TamanoPagina - 1) / TamanoPagina);
    private IEnumerable<FilaHistorialDto> FilasPagina => _resultado?.Filas.Skip(_pagina * TamanoPagina).Take(TamanoPagina) ?? [];

    protected override Task OnInitializedAsync() => CargarAsync();

    private async Task CargarAsync()
    {
        if (Ocupado) return;
        _cargando = true;
        _error = null;
        try
        {
            var opciones = await Servicio.ObtenerOpcionesAsync(_vida.Token);
            if (_vida.IsCancellationRequested) return;
            _opciones = opciones;
            _parque = _opciones.FirstOrDefault()?.ParqueId ?? 0;
            CambiarParque();
        }
        catch (OperationCanceledException) when (_vida.IsCancellationRequested) { }
        catch (Exception ex) { MostrarError(ex, "No se pudo cargar el catálogo. Intentá actualizarlo."); }
        finally { _cargando = false; }
    }

    private void CambiarParque() { _nodo = 0; _panel = 0; }
    private void CambiarNodo() => _panel = 0;
    private void Anterior() => _pagina--;
    private void Siguiente() => _pagina++;

    private async Task ConsultarAsync()
    {
        if (Ocupado || _parque == 0) return;
        _consultando = true;
        _error = null;
        try
        {
            var resultado = await Servicio.ConsultarAsync(_parque, _nodo == 0 ? null : _nodo,
                _panel == 0 ? null : _panel, _periodo, _modo, _vida.Token);
            if (_vida.IsCancellationRequested) return;
            _resultado = resultado;
            _pagina = 0;
        }
        catch (OperationCanceledException) when (_vida.IsCancellationRequested) { }
        catch (Exception ex) { MostrarError(ex, "No se pudo consultar el histórico. Intentá nuevamente."); }
        finally { _consultando = false; }
    }

    private async Task ExportarAsync()
    {
        if (Ocupado || _resultado is null) return;
        _exportando = true;
        _error = null;
        try
        {
            var contenido = await Servicio.ExportarAsync(_resultado);
            if (_vida.IsCancellationRequested) return;
            _modulo ??= await Js.InvokeAsync<IJSObjectReference>("import", _vida.Token, "./js/historial.js");
            using var stream = new MemoryStream(contenido);
            using var referencia = new DotNetStreamReference(stream);
            await _modulo.InvokeVoidAsync("descargarPdf", _vida.Token,
                $"SolarUy-historial-{_resultado.Consulta.Hasta:yyyyMMdd-HHmmss}.pdf", referencia);
        }
        catch (OperationCanceledException) when (_vida.IsCancellationRequested) { }
        catch (Exception ex) { MostrarError(ex, "No se pudo descargar el PDF. Intentá nuevamente."); }
        finally { _exportando = false; }
    }

    private void MostrarError(Exception ex, string mensaje)
    {
        Logger.LogError(ex, "Falló una operación de histórico");
        _error = ex is ArgumentException or UnauthorizedAccessException ? ex.Message : mensaje;
    }

    public async ValueTask DisposeAsync()
    {
        await _vida.CancelAsync();
        if (_modulo is not null)
        {
            try { await _modulo.DisposeAsync(); }
            catch (JSDisconnectedException) { }
        }
        _vida.Dispose();
    }
}
