using PdfSharp.Fonts;

namespace SolarUy.Backend.Modulos.Mediciones.Infraestructura.Exportacion;

public sealed class FuentesHistorial : IFontResolver
{
    public FontResolverInfo ResolveTypeface(string familyName, bool bold, bool italic) => new(bold ? "historial-bold" : "historial");

    public byte[] GetFont(string faceName)
    {
        var bold = faceName == "historial-bold";
        var candidatos = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), bold ? "arialbd.ttf" : "arial.ttf"),
            "/usr/share/fonts/truetype/dejavu/" + (bold ? "DejaVuSans-Bold.ttf" : "DejaVuSans.ttf"),
            "/usr/share/fonts/truetype/liberation2/" + (bold ? "LiberationSans-Bold.ttf" : "LiberationSans-Regular.ttf")
        };
        var ruta = candidatos.FirstOrDefault(File.Exists)
            ?? throw new InvalidOperationException("La exportación PDF requiere Arial, DejaVu Sans o Liberation Sans en el servidor.");
        return File.ReadAllBytes(ruta);
    }
}
