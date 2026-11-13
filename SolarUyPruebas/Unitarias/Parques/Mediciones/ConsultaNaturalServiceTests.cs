using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using PdfSharp.Fonts;
using SolarUy.Backend.Modulos.Consultas.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Consultas.Aplicacion.Implementaciones;
using SolarUy.Backend.Modulos.Consultas.Infraestructura.IA;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Historial;
using SolarUy.Backend.Modulos.Mediciones.Infraestructura.Exportacion;
using SolarUy.Backend.Modulos.Parques.Interfaz.Apis.Local.Contratos;
using SolarUy.Backend.Modulos.Tareas.Aplicacion.DTOs;
using SolarUy.Backend.Modulos.Tareas.Dominio.Enums;

namespace SolarUyPruebas.Unitarias.Mediciones;

public sealed class ConsultaNaturalServiceTests
{
    [Fact]
    public void ParsearPregunta_IdentificaParqueYPeriodoMovil()
    {
        var solicitud = ConsultaNaturalParser.Parsear("¿cómo estuvo la generación del parque 3 en las últimas 24 horas?");

        Assert.Equal(3, solicitud.ParqueId);
        Assert.Equal(PeriodoHistorial.Ultimas24Horas, solicitud.Periodo);
        Assert.Equal(ModoHistorial.Diario, solicitud.Modo);
    }

    [Fact]
    public void ParsearPregunta_IdentificaModoPromedioCuandoSePide()
    {
        var solicitud = ConsultaNaturalParser.Parsear("quiero el promedio del parque 5 en los últimos 30 días");

        Assert.Equal(5, solicitud.ParqueId);
        Assert.Equal(PeriodoHistorial.Ultimos30Dias, solicitud.Periodo);
        Assert.Equal(ModoHistorial.Promedio, solicitud.Modo);
    }

    [Fact]
    public void ParsearPregunta_ResuelveParquePorNombreNatural()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 1),
            new OpcionHistorialDto(2, "Parque 2", 2, "NOD 12", 4),
        };

        var solicitud = ConsultaNaturalParser.Parsear("¿cómo estuvo la generación del parque prueba 1 en las últimas 24 horas?", opciones);

        Assert.Equal(1, solicitud.ParqueId);
        Assert.Equal(PeriodoHistorial.Ultimas24Horas, solicitud.Periodo);
        Assert.Equal(ModoHistorial.Diario, solicitud.Modo);
    }

    [Fact]
    public void ParsearPregunta_ResuelveNodoConAliasNatural()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 1),
            new OpcionHistorialDto(1, "Parque Prueba 1", 2, "NOD 12", 4),
            new OpcionHistorialDto(2, "Parque 2", 3, "NOD 20", 3),
        };

        var solicitud = ConsultaNaturalParser.Parsear("quiero la generación del parque prueba 1 en el nodo nod 12 en las últimas 24 horas", opciones);

        Assert.Equal(1, solicitud.ParqueId);
        Assert.Equal(2, solicitud.NodoId);
        Assert.Equal(PeriodoHistorial.Ultimas24Horas, solicitud.Periodo);
    }

    [Fact]
    public void ConsultaNaturalService_RespondeCantidadDeParquesDisponibles()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 1),
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 2),
            new OpcionHistorialDto(2, "Parque 2", 2, "NOD 12", 4),
            new OpcionHistorialDto(2, "Parque 2", 3, "NOD 20", 3),
        };

        var respuesta = ConsultaNaturalService.ResponderCatalogo(opciones, "cuantos parques hay disponibles?");

        Assert.Contains("2 parques", respuesta, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParsearPregunta_ResuelvePanelSinParqueExplicito()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 1),
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 2),
            new OpcionHistorialDto(2, "Parque 2", 2, "NOD 12", 4),
            new OpcionHistorialDto(2, "Parque 2", 3, "NOD 20", 3),
        };

        var solicitud = ConsultaNaturalParser.Parsear("dame la generacion del panel 4", opciones);

        Assert.Equal(2, solicitud.ParqueId);
        Assert.Equal(2, solicitud.NodoId);
        Assert.Equal(4, solicitud.PanelId);
    }

    [Fact]
    public void ConsultaNaturalService_RespondeParqueConMasNodos()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 1),
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 2),
            new OpcionHistorialDto(2, "Parque 2", 2, "NOD 12", 4),
            new OpcionHistorialDto(2, "Parque 2", 3, "NOD 20", 3),
        };

        var respuesta = ConsultaNaturalService.ResponderCatalogo(opciones, "cual es el parque con mas nodos?");

        Assert.Contains("Parque 2", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2 nodos", respuesta, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsultaNaturalService_RespondeNodoConMasPaneles()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 1),
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 2),
            new OpcionHistorialDto(2, "Parque 2", 2, "NOD 12", 4),
            new OpcionHistorialDto(2, "Parque 2", 3, "NOD 20", 3),
        };

        var respuesta = ConsultaNaturalService.ResponderCatalogo(opciones, "cual es el nodo con mas paneles?");

        Assert.Contains("Nodo 1", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2 paneles", respuesta, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsultaNaturalService_RespondeOrientacionDePaneles()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 1),
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 2),
            new OpcionHistorialDto(2, "Parque 2", 2, "NOD 12", 4),
        };
        var catalogo = new[]
        {
            new ParqueProduccion(1, "Parque Prueba 1", new[]
            {
                new NodoProduccion(1, "Nodo 1", "Centro", new[]
                {
                    new PanelProduccion(1, 110, "Norte", "Bueno", false),
                    new PanelProduccion(2, 90, "Sur", "Intermedio", false)
                })
            }),
            new ParqueProduccion(2, "Parque 2", new[]
            {
                new NodoProduccion(2, "NOD 12", "Este", new[]
                {
                    new PanelProduccion(4, 75, "Este", "Bueno", false)
                })
            })
        };

        var respuesta = ConsultaNaturalService.ResponderCatalogo(opciones, "qué orientación tienen los paneles?", catalogo);

        Assert.Contains("Norte", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Sur", respuesta, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsultaNaturalService_RespondePanelMasProductivo()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 1),
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 2),
            new OpcionHistorialDto(2, "Parque 2", 2, "NOD 12", 4),
        };
        var catalogo = new[]
        {
            new ParqueProduccion(1, "Parque Prueba 1", new[]
            {
                new NodoProduccion(1, "Nodo 1", "Centro", new[]
                {
                    new PanelProduccion(1, 15, "Norte", "Bueno", false),
                    new PanelProduccion(2, 30, "Sur", "Bueno", false)
                })
            }),
            new ParqueProduccion(2, "Parque 2", new[]
            {
                new NodoProduccion(2, "NOD 12", "Este", new[]
                {
                    new PanelProduccion(4, 8, "Este", "Bueno", false)
                })
            })
        };

        var respuesta = ConsultaNaturalService.ResponderCatalogo(opciones, "Qué panel produce más?", catalogo);

        Assert.Contains("Panel #2", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("30", respuesta, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsultaNaturalService_RespondeEstadoBuenoDePaneles()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 1),
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 2),
        };
        var catalogo = new[]
        {
            new ParqueProduccion(1, "Parque Prueba 1", new[]
            {
                new NodoProduccion(1, "Nodo 1", "Centro", new[]
                {
                    new PanelProduccion(1, 15, "Norte", "Bueno", false),
                    new PanelProduccion(2, 30, "Sur", "Intermedio", false)
                })
            })
        };

        var respuesta = ConsultaNaturalService.ResponderCatalogo(opciones, "Qué paneles están en estado bueno?", catalogo);

        Assert.Contains("Panel #1", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Panel #2", respuesta, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsultaNaturalService_RespondeOrientacionEspecificaDePanel()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 1),
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 2),
        };
        var catalogo = new[]
        {
            new ParqueProduccion(1, "Parque Prueba 1", new[]
            {
                new NodoProduccion(1, "Nodo 1", "Centro", new[]
                {
                    new PanelProduccion(1, 15, "Este", "Bueno", false),
                    new PanelProduccion(2, 30, "Oeste", "Intermedio", false)
                })
            })
        };

        var respuesta = ConsultaNaturalService.ResponderCatalogo(opciones, "Que orientacion tiene el panel 1", catalogo);

        Assert.Contains("Este", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Oeste", respuesta, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsultaNaturalService_RespondePanelesEnEstadoNoBueno()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 1),
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 2),
            new OpcionHistorialDto(2, "Parque 2", 2, "NOD 12", 4),
        };
        var catalogo = new[]
        {
            new ParqueProduccion(1, "Parque Prueba 1", new[]
            {
                new NodoProduccion(1, "Nodo 1", "Centro", new[]
                {
                    new PanelProduccion(1, 15, "Este", "Bueno", false),
                    new PanelProduccion(2, 30, "Oeste", "Intermedio", false)
                })
            }),
            new ParqueProduccion(2, "Parque 2", new[]
            {
                new NodoProduccion(2, "NOD 12", "Este", new[]
                {
                    new PanelProduccion(4, 8, "Este", "Bueno", false)
                })
            })
        };

        var respuesta = ConsultaNaturalService.ResponderCatalogo(opciones, "algun panel se encuentra en estado no bueno?", catalogo);

        Assert.Contains("Intermedio", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Panel #1", respuesta, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsultaNaturalService_RespondePanelesEnEstadoIntermedio()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 1),
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 2),
        };
        var catalogo = new[]
        {
            new ParqueProduccion(1, "Parque Prueba 1", new[]
            {
                new NodoProduccion(1, "Nodo 1", "Centro", new[]
                {
                    new PanelProduccion(1, 15, "Este", "Bueno", false),
                    new PanelProduccion(2, 30, "Oeste", "Intermedio", false)
                })
            })
        };

        var respuesta = ConsultaNaturalService.ResponderCatalogo(opciones, "hay paneles en estado intermedio?", catalogo);

        Assert.Contains("Intermedio", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Panel #2", respuesta, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsultaNaturalService_RespondeListaDeNodosEnLugarDeOpcionesGenericas()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 1),
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 2),
            new OpcionHistorialDto(2, "Parque 2", 2, "NOD 12", 4),
            new OpcionHistorialDto(2, "Parque 2", 3, "NOD 20", 3),
        };

        var respuesta = ConsultaNaturalService.ResponderCatalogo(opciones, "dame la lista de nodos");

        Assert.Contains("Nodo #1", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("NOD 12", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Opciones disponibles", respuesta, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsultaNaturalService_FiltraListasPorParqueYNodoCuandoSeAcotaLaConsulta()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 1),
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 2),
            new OpcionHistorialDto(1, "Parque Prueba 1", 2, "Nodo 2", 3),
            new OpcionHistorialDto(2, "Parque 2", 3, "NOD 12", 4),
            new OpcionHistorialDto(2, "Parque 2", 4, "NOD 20", 5),
        };

        var nodosDelParque1 = ConsultaNaturalService.ResponderCatalogo(opciones, "dame la lista de nodos del parque 1");
        var panelesDelParque2 = ConsultaNaturalService.ResponderCatalogo(opciones, "dame la lista de paneles del parque 2");
        var panelesDelNodo1 = ConsultaNaturalService.ResponderCatalogo(opciones, "dame los paneles del nodo 1 del parque 1");

        Assert.Contains("Nodo #1", nodosDelParque1, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Nodo #2", nodosDelParque1, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("NOD 12", nodosDelParque1, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("Panel #4", panelesDelParque2, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Panel #1", panelesDelParque2, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("Panel #1", panelesDelNodo1, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Panel #2", panelesDelNodo1, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Panel #3", panelesDelNodo1, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsultaNaturalService_RespondeConteoDeNodosYPanelesPorContexto()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 1),
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 2),
            new OpcionHistorialDto(1, "Parque Prueba 1", 2, "Nodo 2", 3),
            new OpcionHistorialDto(2, "Parque 2", 3, "NOD 12", 4),
            new OpcionHistorialDto(2, "Parque 2", 4, "NOD 20", 5),
        };

        var nodosDelParque1 = ConsultaNaturalService.ResponderCatalogo(opciones, "cuantos nodos tiene el parque 1?");
        var panelesDelNodo1 = ConsultaNaturalService.ResponderCatalogo(opciones, "Cuantos paneles tiene el nodo 1?");
        var panelesDelParque1 = ConsultaNaturalService.ResponderCatalogo(opciones, "Cuantos paneles tiene el parque 1?");

        Assert.Contains("2 nodos", nodosDelParque1, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Parque #1", nodosDelParque1, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("2 paneles", panelesDelNodo1, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Nodo #1", panelesDelNodo1, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("3 paneles", panelesDelParque1, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Parque #1", panelesDelParque1, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParsearPregunta_IdentificaPromedioDelParqueSinListarOpciones()
    {
        var solicitud = ConsultaNaturalParser.Parsear("dame el promedio del parque 1");

        Assert.Equal(1, solicitud.ParqueId);
        Assert.Equal(ModoHistorial.Promedio, solicitud.Modo);
        Assert.False(solicitud.EsListado);
    }

    [Fact]
    public void ConsultaNaturalService_ResponderTareas_FiltraPorPanelSolicitado()
    {
        var tareas = new[]
        {
            new TareaDto(1, 4, TipoMantenimiento.Limpieza, new DateTime(2026, 11, 12), null, EstadoTarea.Pendiente, string.Empty, null),
            new TareaDto(2, 3, TipoMantenimiento.Reparacion, new DateTime(2026, 11, 11), null, EstadoTarea.Pendiente, string.Empty, null)
        };

        var respuestaPanel1 = ConsultaNaturalService.ResponderTareas(tareas, "Hay tareas pendientes para el panel 1?");
        var respuestaPanel4 = ConsultaNaturalService.ResponderTareas(tareas, "Hay tareas pendientes para el panel 4?");

        Assert.Contains("No hay tareas pendientes", respuestaPanel1, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Tarea #1", respuestaPanel4, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Tarea #2", respuestaPanel4, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsultaNaturalService_RespondeProduccionDePanelEspecifico()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 1),
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 2),
            new OpcionHistorialDto(2, "Parque 2", 2, "NOD 12", 4),
            new OpcionHistorialDto(2, "Parque 2", 3, "NOD 20", 3),
        };
        var catalogo = new[]
        {
            new ParqueProduccion(1, "Parque Prueba 1", new[]
            {
                new NodoProduccion(1, "Nodo 1", "Centro", new[]
                {
                    new PanelProduccion(1, 120, "Norte", "Bueno", false),
                    new PanelProduccion(2, 90, "Sur", "Intermedio", false)
                })
            }),
            new ParqueProduccion(2, "Parque 2", new[]
            {
                new NodoProduccion(2, "NOD 12", "Este", new[]
                {
                    new PanelProduccion(4, 75, "Este", "Bueno", false)
                }),
                new NodoProduccion(3, "NOD 20", "Oeste", new[]
                {
                    new PanelProduccion(3, 52, "Oeste", "Bueno", false)
                })
            })
        };

        var respuesta = ConsultaNaturalService.ResponderCatalogo(opciones, "dame la produccion del panel 3", catalogo);

        Assert.Contains("Panel #3", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("52", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Opciones disponibles", respuesta, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsultaNaturalService_RespondePotenciaActualDeCadaPanel()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 1),
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 2),
            new OpcionHistorialDto(2, "Parque 2", 2, "NOD 12", 4),
        };
        var catalogo = new[]
        {
            new ParqueProduccion(1, "Parque Prueba 1", new[]
            {
                new NodoProduccion(1, "Nodo 1", "Centro", new[]
                {
                    new PanelProduccion(1, 120, "Norte", "Bueno", false),
                    new PanelProduccion(2, 90, "Sur", "Intermedio", false)
                })
            }),
            new ParqueProduccion(2, "Parque 2", new[]
            {
                new NodoProduccion(2, "NOD 12", "Este", new[]
                {
                    new PanelProduccion(4, 75, "Este", "Bueno", false)
                })
            })
        };

        var respuesta = ConsultaNaturalService.ResponderCatalogo(opciones, "dame la potencia actual de cada panel", catalogo);

        Assert.Contains("Panel #1", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("120", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Panel #2", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Opciones disponibles", respuesta, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsultaNaturalService_RespondePanelConMasPotencia()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 1),
            new OpcionHistorialDto(1, "Parque Prueba 1", 1, "Nodo 1", 2),
            new OpcionHistorialDto(2, "Parque 2", 2, "NOD 12", 4),
        };
        var catalogo = new[]
        {
            new ParqueProduccion(1, "Parque Prueba 1", new[]
            {
                new NodoProduccion(1, "Nodo 1", "Centro", new[]
                {
                    new PanelProduccion(1, 120, "Norte", "Bueno", false),
                    new PanelProduccion(2, 90, "Sur", "Intermedio", false)
                })
            }),
            new ParqueProduccion(2, "Parque 2", new[]
            {
                new NodoProduccion(2, "NOD 12", "Este", new[]
                {
                    new PanelProduccion(4, 75, "Este", "Bueno", false)
                })
            })
        };

        var respuesta = ConsultaNaturalService.ResponderCatalogo(opciones, "cuál panel tiene más potencia?", catalogo);

        Assert.Contains("Panel #1", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("120", respuesta, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParsearPregunta_AceptaConsultasDeTareasComoLectura()
    {
        var solicitud = ConsultaNaturalParser.Parsear("Hay tareas pendientes?");

        Assert.NotNull(solicitud);
        Assert.True(solicitud.ParqueId is null || solicitud.ParqueId <= 0);
    }

    [Fact]
    public void ConsultaNaturalService_RespondeResumenDeTareasPendientesYCompletadas()
    {
        var tareas = new[]
        {
            new TareaDto(1, 10, TipoMantenimiento.Limpieza, DateTime.UtcNow.AddDays(-2), null, EstadoTarea.Pendiente, "Limpieza", null),
            new TareaDto(2, 11, TipoMantenimiento.Reparacion, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-4), EstadoTarea.Completada, "Reparación", null),
            new TareaDto(3, 12, TipoMantenimiento.Limpieza, DateTime.UtcNow.AddDays(-1), null, EstadoTarea.Pendiente, "Limpieza", null)
        };

        var respuesta = ConsultaNaturalService.ResponderTareas(tareas, "Cuantas tareas pendientes hay?");

        Assert.Contains("2", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("pendientes", respuesta, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsultaNaturalService_RespondeDetalleDeTareasCuandoSePideListaODetalle()
    {
        var tareas = new[]
        {
            new TareaDto(1, 10, TipoMantenimiento.Limpieza, new DateTime(2026, 1, 5), null, EstadoTarea.Pendiente, "Limpieza del panel", null),
            new TareaDto(2, 11, TipoMantenimiento.Reparacion, new DateTime(2026, 1, 7), new DateTime(2026, 1, 9), EstadoTarea.Completada, "Revisión del inversor", null)
        };

        var respuesta = ConsultaNaturalService.ResponderTareas(tareas, "dame la lista de tareas");

        Assert.Contains("Tarea #1", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Limpieza", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Panel #10", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Reparación", respuesta, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Completada", respuesta, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProveedorConsultaLocal_SinOllama_NoDevuelveTextoGenerico()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Consultas:Proveedor"] = "local"
            })
            .Build();
        var proveedor = new ProveedorConsultaLocal(config, new HttpClient());

        var respuesta = await proveedor.ObtenerRespuestaAsync("hola");

        Assert.DoesNotContain("Consulta preparada con el motor local de SolarUy", respuesta, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParsearPregunta_RechazaSolicitudesDeModificacion()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            ConsultaNaturalParser.Parsear("creá un parque y cambia el panel 7"));

        Assert.Contains("solo consulta", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsultaNaturalService_DetectaPedidoDeDescargaPdf()
    {
        Assert.True(ConsultaNaturalService.EsPedidoExportacionPdf("descargá el pdf del parque 2 en las últimas 24 horas"));
        Assert.True(ConsultaNaturalService.EsPedidoExportacionPdf("quiero exportar el historial a pdf"));
        Assert.True(ConsultaNaturalService.EsPedidoExportacionPdf("Dame un pdf de la produccion historica del nodo 6"));
        Assert.False(ConsultaNaturalService.EsPedidoExportacionPdf("¿cómo estuvo la generación del parque 2 en las últimas 24 horas?"));
    }

    [Fact]
    public void ParsearPregunta_ResuelveNodoConPdfHistorico()
    {
        var opciones = new[]
        {
            new OpcionHistorialDto(3, "Par 3", 4, "NOD 0303", 9),
            new OpcionHistorialDto(3, "Par 3", 5, "NOD 0301", 5),
            new OpcionHistorialDto(3, "Par 3", 6, "NOD 0302", 7),
        };

        var solicitud = ConsultaNaturalParser.Parsear("Dame un pdf de la produccion historica del nodo 6", opciones);

        Assert.Equal(3, solicitud.ParqueId);
        Assert.Equal(6, solicitud.NodoId);
        Assert.Equal(PeriodoHistorial.Todos, solicitud.Periodo);
        Assert.Equal(ModoHistorial.Diario, solicitud.Modo);
    }

    [Fact]
    public void FuentesHistorial_ConfiguraResolverYResuelveFuentesDelPdf()
    {
        FuentesHistorial.Configurar();

        var resolver = Assert.IsType<FuentesHistorial>(GlobalFontSettings.FontResolver);
        var tipo = resolver.ResolveTypeface("Courier New", false, false);
        var fuente = resolver.GetFont(tipo.FaceName);

        Assert.NotNull(tipo);
        Assert.Equal("historial", tipo.FaceName, ignoreCase: true);
        Assert.NotEmpty(fuente);

        var consulta = new ConsultaHistorial(3, 5, 8, PeriodoHistorial.Todos, ModoHistorial.Diario, new DateTimeOffset(2026, 10, 15, 12, 0, 0, TimeSpan.Zero));
        var constructor = typeof(ResultadoHistorialDto).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            new[] { typeof(ConsultaHistorial), typeof(string), typeof(IEnumerable<FilaHistorialDto>) },
            modifiers: null);

        Assert.NotNull(constructor);

        var resultado = (ResultadoHistorialDto)constructor!.Invoke([
            consulta,
            "Parque 3 / Nodo 5 / Panel 8",
            new[]
            {
                new FilaHistorialDto(1, 3, 5, 8, new DateTimeOffset(2026, 10, 10, 8, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 10, 10, 8, 15, 0, TimeSpan.Zero), new DateTime(2026, 10, 10), 150.60, 12),
                new FilaHistorialDto(2, 3, 5, 8, new DateTimeOffset(2026, 10, 11, 8, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 10, 11, 8, 20, 0, TimeSpan.Zero), new DateTime(2026, 10, 11), 172.30, 9)
            }
        ]);

        var pdf = new ExportadorHistorialPdf().Generar(resultado);

        Assert.NotEmpty(pdf);
        Assert.Equal("%PDF", Encoding.UTF8.GetString(pdf.Take(4).ToArray()));
    }

    [Fact]
    public void ParsearPregunta_RechazaTemaFueraDelSistema()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            ConsultaNaturalParser.Parsear("¿cuál es la capital de Argentina?"));

        Assert.Contains("fuera de tema", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
