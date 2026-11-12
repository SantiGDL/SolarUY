using System.Net.Http.Json;
using System.Text.Json;
using SolarUy.Backend.Modulos.Consultas.Aplicacion.Contratos;

namespace SolarUy.Backend.Modulos.Consultas.Infraestructura.IA;

public sealed class ProveedorConsultaLocal(IConfiguration configuration, HttpClient httpClient) : IProveedorConsultaIA
{
    public async Task<string> ObtenerRespuestaAsync(string pregunta, CancellationToken cancellationToken = default)
    {
        var proveedor = configuration["Consultas:Proveedor"] ?? configuration["IA:Proveedor"] ?? "local";
        if (!string.Equals(proveedor, "ollama", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        var endpoint = configuration["Consultas:OllamaUrl"] ?? configuration["IA:OllamaUrl"] ?? "http://localhost:11434/api/chat";
        var modelo = configuration["Consultas:Modelo"] ?? configuration["IA:Modelo"] ?? "llama3.2";

        try
        {
            var cuerpo = new
            {
                model = modelo,
                stream = false,
                messages = new[]
                {
                    new { role = "user", content = pregunta }
                }
            };

            using var respuesta = await httpClient.PostAsJsonAsync(endpoint, cuerpo, cancellationToken);
            if (!respuesta.IsSuccessStatusCode)
                throw new InvalidOperationException($"Ollama respondió con estado {(int)respuesta.StatusCode}.");

            var json = await respuesta.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            if (json.TryGetProperty("message", out var mensaje) &&
                mensaje.TryGetProperty("content", out var contenido))
            {
                return contenido.GetString() ?? "No pude interpretar la respuesta del proveedor local.";
            }

            return "No pude interpretar la respuesta del modelo local.";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or NotSupportedException)
        {
            throw new InvalidOperationException("El proveedor de IA local no está disponible. Revisa la configuración de Ollama o vuelve a usar la consulta basada en datos históricos.", ex);
        }
    }
}
