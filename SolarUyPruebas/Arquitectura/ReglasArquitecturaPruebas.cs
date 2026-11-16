using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using SolarUy.Backend.Modulos.Parques.Dominio.Entidades;
using Xunit;
using Xunit.Abstractions;
using static SolarUyArquitecturaPruebas.ContratosLocales;

namespace SolarUyArquitecturaPruebas;

public sealed class ReglasArquitecturaPruebas(ITestOutputHelper salida)
{
    private static readonly System.Reflection.Assembly SolarUy = typeof(Parque).Assembly;
    private static readonly Architecture Arquitectura = new ArchLoader().LoadAssemblies(SolarUy).Build();
    private static readonly IType[] Tipos = Arquitectura.Types
        .Where(tipo => tipo.Assembly.Name == SolarUy.GetName().Name).ToArray();
    private static readonly ContratosLocales Contratos = new(SolarUy.GetTypes());

    [Theory]
    [InlineData("SolarUy.Backend.Modulos.Parques.Dominio", "Parque")]
    [InlineData("SolarUy.Backend.Modulos.Parques.Aplicacion", "ParquesService")]
    [InlineData("SolarUy.Backend.Modulos.Parques.Infraestructura", "ParqueRepositorio")]
    [InlineData("SolarUy.Backend.Modulos.Parques.Interfaz.Apis.Local.Contratos", "ICatalogoProduccion")]
    [InlineData("SolarUy.Backend.Modulos.Tareas.Dominio", "TareaMantenimiento")]
    [InlineData("SolarUy.Backend.Modulos.Tareas.Aplicacion", "TareasService")]
    [InlineData("SolarUy.Backend.Modulos.Tareas.Infraestructura", "TareaRepositorio")]
    [InlineData("SolarUy.Backend.Modulos.Mediciones.Dominio", "Medicion")]
    [InlineData("SolarUy.Backend.Modulos.Mediciones.Aplicacion", "MedicionesService")]
    [InlineData("SolarUy.Backend.Modulos.Mediciones.Infraestructura", "MedicionesDbContext")]
    [InlineData("SolarUy.Backend.Modulos.Mediciones.Interfaz", "MedicionesHub")]
    [InlineData("SolarUy.Backend.Modulos.Usuarios.Dominio", "Usuario")]
    [InlineData("SolarUy.Backend.Modulos.Consultas.Aplicacion", "ConsultaNaturalService")]
    [InlineData("SolarUy.Backend.Modulos.Consultas.Infraestructura", "ProveedorConsultaLocal")]
    [InlineData("SolarUy.Backend.Infraestructura", "ParquesDbContext")]
    [InlineData("SolarUy.Components.Pages", "HistorialMediciones")]
    [InlineData("SolarUy.Components.Layout", "MainLayout")]
    public void ElAnalisisIncluyeLosTiposEsperados(string espacio, string nombre) =>
        Assert.True(Tipos.Any(tipo => EnNamespace(Namespace(tipo), espacio) &&
                                     tipo.Name == nombre),
            $"No se encontró {espacio} {nombre} en el ensamblado analizado.");

    [Fact]
    public void LosModulosUsanSoloLosTiposExpuestosDeOtrosModulos()
    {
        foreach (var observacion in Contratos.Observaciones)
        {
            salida.WriteLine(observacion);
        }

        Comprobar("ARQ-01", tipo => Modulo(Namespace(tipo)) is not null, (origen, destino) =>
            destino.Assembly.Name == SolarUy.GetName().Name &&
            Modulo(Namespace(destino)) is { } moduloDestino &&
            moduloDestino != Modulo(Namespace(origen)) &&
            !Contratos.EstaExpuesto(destino.FullName));
    }

    [Fact]
    public void DominioEsIndependienteDeLasCapasExternas() =>
        Comprobar("ARQ-02", tipo => Capa(tipo, "Dominio"), (_, destino) =>
            Capa(destino, "Aplicacion") || EsInfraestructura(destino) ||
            Capa(destino, "Interfaz") || EsPresentacion(destino) ||
            EnNamespace(Namespace(destino), "Microsoft.AspNetCore") ||
            EsEntityFramework(destino));

    [Fact]
    public void AplicacionNoDependeDePresentacionNiDeInfraestructura() =>
        Comprobar("ARQ-03", tipo => Capa(tipo, "Aplicacion"), (_, destino) =>
            EsPresentacion(destino) || EsInfraestructura(destino) || EsEntityFramework(destino) ||
            EnNamespace(Namespace(destino), "Microsoft.AspNetCore.Components") ||
            destino.FullName == "Microsoft.AspNetCore.Http.HttpContext");

    [Fact]
    public void PresentacionNoAccedeDirectamenteAPersistenciaNiAInfraestructura() =>
        Comprobar("ARQ-04", EsPresentacion, (_, destino) =>
            EsInfraestructura(destino) || EsEntityFramework(destino) ||
            Capa(destino, "Dominio.Repositorios") ||
            (Capa(destino, "Aplicacion.Contratos") &&
             (destino.Name.EndsWith("Repositorio", StringComparison.Ordinal) ||
              destino.Name.EndsWith("Repository", StringComparison.Ordinal))));

    [Fact]
    public void LaComposicionEnProgramPuedeRegistrarInfraestructura()
    {
        // Program contiene el punto de entrada generado para las instrucciones de nivel superior.
        var entrada = SolarUy.EntryPoint?.DeclaringType;
        Assert.Equal("Program", entrada?.FullName);
        var programa = Assert.Single(Tipos, tipo => tipo.FullName == entrada!.FullName);
        Assert.True(EsComposicion(programa));
        Assert.False(EsPresentacion(programa));
        Assert.DoesNotContain(Tipos.Where(EsComposicion),
            tipo => EnNamespace(Namespace(tipo), "SolarUy.Components"));
    }

    private static void Comprobar(
        string regla, Func<IType, bool> seleccionarOrigen, Func<IType, IType, bool> prohibida)
    {
        var origenes = Tipos.Where(seleccionarOrigen).ToArray();
        Assert.True(origenes.Length > 0, $"{regla}: la selección de tipos de origen está vacía.");
        var infracciones = origenes
            .SelectMany(origen => Dependencias(origen)
                .Where(destino => prohibida(origen, destino))
                .Select(destino => $"{origen.FullName} → {destino.FullName}"))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(infracciones.Length == 0,
            $"{regla}: {infracciones.Length} dependencias prohibidas{Environment.NewLine}" +
            string.Join(Environment.NewLine, infracciones));
    }

    private static IEnumerable<IType> Dependencias(IType origen) =>
        origen.Dependencies.SelectMany(dependencia =>
            TiposGenericos(dependencia.TargetGenericArguments).Prepend(dependencia.Target));

    private static IEnumerable<IType> TiposGenericos(IEnumerable<GenericArgument> argumentos) =>
        argumentos.SelectMany(argumento =>
            TiposGenericos(argumento.GenericArguments).Prepend(argumento.Type));

    private static string Namespace(IType tipo) => tipo.Namespace.FullName;

    private static bool Capa(IType tipo, string capa) =>
        EnNamespace(RelativoAlModulo(Namespace(tipo)), capa);

    private static bool EsInfraestructura(IType tipo) =>
        Capa(tipo, "Infraestructura") ||
        Capa(tipo, "Interfaz.Apis.Local.Implementaciones") ||
        EnNamespace(Namespace(tipo), "SolarUy.Backend.Infraestructura");

    private static bool EsComposicion(IType tipo) =>
        tipo.FullName == SolarUy.EntryPoint?.DeclaringType?.FullName;

    private static bool EsPresentacion(IType tipo) =>
        !EsComposicion(tipo) &&
        (EnNamespace(Namespace(tipo), "SolarUy.Components") ||
         (Capa(tipo, "Interfaz") && !Capa(tipo, "Interfaz.Apis.Local.Contratos") &&
          !Capa(tipo, "Interfaz.Eventos.OUT") && !EsInfraestructura(tipo)));

    private static bool EsEntityFramework(IType tipo) =>
        EnNamespace(Namespace(tipo), "Microsoft.EntityFrameworkCore");
}
