using PdfSharp.Fonts;

namespace SolarUy.Backend.Modulos.Mediciones.Infraestructura.Exportacion;

public sealed class FuentesHistorial : IFontResolver
{
    public static void Configurar()
    {
        GlobalFontSettings.FontResolver = new FuentesHistorial();
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool bold, bool italic)
    {
        return new FontResolverInfo(bold ? "historial-bold" : "historial");
    }

    public byte[] GetFont(string faceName)
    {
        var nombre = string.IsNullOrWhiteSpace(faceName) ? "historial" : faceName.Trim();
        var esNegrita = nombre.Equals("historial-bold", StringComparison.OrdinalIgnoreCase)
            || nombre.Equals("bold", StringComparison.OrdinalIgnoreCase);

        if (nombre.Equals("courier new", StringComparison.OrdinalIgnoreCase)
            || nombre.Equals("courier", StringComparison.OrdinalIgnoreCase)
            || nombre.Equals("arial", StringComparison.OrdinalIgnoreCase)
            || nombre.Equals("helvetica", StringComparison.OrdinalIgnoreCase)
            || nombre.Equals("times new roman", StringComparison.OrdinalIgnoreCase)
            || nombre.Equals("times", StringComparison.OrdinalIgnoreCase)
            || nombre.Equals("historial", StringComparison.OrdinalIgnoreCase))
        {
            nombre = esNegrita ? "historial-bold" : "historial";
        }

        esNegrita = nombre.Equals("historial-bold", StringComparison.OrdinalIgnoreCase);

        var rutasBase = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
            "/usr/share/fonts",
            "/usr/local/share/fonts",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts")
        };

        var nombresCandidatos = esNegrita
            ? new[] { "arialbd.ttf", "Arial Bold.ttf", "Arial-Bold.ttf", "LiberationSans-Bold.ttf", "DejaVuSans-Bold.ttf" }
            : new[] { "arial.ttf", "Arial.ttf", "LiberationSans-Regular.ttf", "DejaVuSans.ttf" };

        var ruta = rutasBase
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .SelectMany(r => Directory.Exists(r)
                ? nombresCandidatos.SelectMany(c => Directory.EnumerateFiles(r, c, SearchOption.AllDirectories))
                : Enumerable.Empty<string>())
            .FirstOrDefault(File.Exists);

        if (ruta is null)
        {
            var alternativo = rutasBase
                .Where(r => !string.IsNullOrWhiteSpace(r) && Directory.Exists(r))
                .SelectMany(r => Directory.EnumerateFiles(r, "*.ttf", SearchOption.AllDirectories))
                .Concat(rutasBase
                    .Where(r => !string.IsNullOrWhiteSpace(r) && Directory.Exists(r))
                    .SelectMany(r => Directory.EnumerateFiles(r, "*.otf", SearchOption.AllDirectories)))
                .FirstOrDefault();

            if (alternativo is null)
            {
                throw new InvalidOperationException("La exportación PDF requiere Arial, DejaVu Sans o Liberation Sans en el servidor.");
            }

            ruta = alternativo;
        }

        return File.ReadAllBytes(ruta);
    }
}
