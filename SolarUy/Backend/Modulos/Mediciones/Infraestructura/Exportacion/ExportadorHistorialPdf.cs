using System.Globalization;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Historial;

namespace SolarUy.Backend.Modulos.Mediciones.Infraestructura.Exportacion;

public sealed class ExportadorHistorialPdf : IExportadorHistorialPdf
{
    public byte[] Generar(ResultadoHistorialDto resultado)
    {
        var documento = new Document();
        documento.Info.Title = "SolarUy - Historial de mediciones";
        var normal = documento.Styles[StyleNames.Normal]!;
        normal.Font.Name = "Historial";
        normal.Font.Size = 9;
        var seccion = documento.AddSection();
        seccion.PageSetup.PageFormat = PageFormat.A4;
        seccion.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
        seccion.PageSetup.RightMargin = Unit.FromCentimeter(1.5);
        seccion.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
        seccion.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
        var titulo = seccion.AddParagraph("SolarUy | Historial de mediciones");
        titulo.Format.Font.Size = 18;
        titulo.Format.Font.Bold = true;
        titulo.Format.Font.Color = Color.FromRgb(24, 96, 66);
        titulo.Format.SpaceAfter = 12;
        seccion.AddParagraph(resultado.Seleccion);
        seccion.AddParagraph($"{resultado.Periodo} · {resultado.Modo}");
        seccion.AddParagraph($"Desde: {(resultado.Consulta.Desde.HasValue ? ResultadoHistorialDto.FechaLocal(resultado.Consulta.Desde.Value) : "Primer registro disponible")}");
        seccion.AddParagraph($"Hasta: {ResultadoHistorialDto.FechaLocal(resultado.Consulta.Hasta)} · Uruguay (UTC-03:00)");
        var aviso = seccion.AddParagraph(ResultadoHistorialDto.Aviso);
        aviso.Format.SpaceBefore = 8;
        aviso.Format.SpaceAfter = 12;
        aviso.Format.Font.Size = 8;
        var tabla = seccion.AddTable();
        tabla.Borders.Width = 0.4;
        tabla.Borders.Color = Colors.LightGray;
        foreach (var ancho in new[] { 3.1, 2.5, 2.5, 2.5, 2.4, 2.0, 3.0 }) tabla.AddColumn(Unit.FromCentimeter(ancho));
        var cabecera = tabla.AddRow();
        cabecera.HeadingFormat = true;
        cabecera.Shading.Color = Color.FromRgb(232, 245, 239);
        cabecera.Format.Font.Bold = true;
        Escribir(cabecera, ["Fecha / día", "Parque", "Nodo", "Panel", "Potencia (W)", "Muestras", "Registro / última lectura"]);
        foreach (var fila in resultado.Filas)
        {
            var renglon = tabla.AddRow();
            renglon.TopPadding = 4;
            renglon.BottomPadding = 4;
            Escribir(renglon, [fila.Dia?.ToString("dd/MM/yyyy") ?? ResultadoHistorialDto.FechaLocal(fila.PrimeraFecha),
                $"#{fila.ParqueId}", $"#{fila.NodoId}", $"#{fila.PanelId}",
                fila.Valor.ToString("N2", CultureInfo.GetCultureInfo("es-UY")), fila.Cantidad.ToString(),
                resultado.Consulta.Modo == ModoHistorial.Crudos ? $"#{fila.Id}" : ResultadoHistorialDto.FechaLocal(fila.UltimaFecha)]);
        }
        if (resultado.Filas.Count == 0) seccion.AddParagraph("No hay mediciones para esta consulta.");
        var pie = seccion.Footers.Primary.AddParagraph();
        pie.Format.Alignment = ParagraphAlignment.Right;
        pie.AddText("SolarUy · Página ");
        pie.AddPageField();
        pie.AddText(" de ");
        pie.AddNumPagesField();
        var renderer = new PdfDocumentRenderer { Document = documento };
        renderer.RenderDocument();
        using var salida = new MemoryStream();
        renderer.PdfDocument.Save(salida, false);
        return salida.ToArray();
    }

    private static void Escribir(Row fila, string[] valores)
    {
        for (var indice = 0; indice < valores.Length; indice++) fila.Cells[indice].AddParagraph(valores[indice]);
    }
}
