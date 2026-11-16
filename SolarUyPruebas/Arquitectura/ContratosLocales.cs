using System.Reflection;

namespace SolarUyArquitecturaPruebas;

internal sealed class ContratosLocales
{
    internal const string PrefijoModulos = "SolarUy.Backend.Modulos.";
    private const BindingFlags MiembrosPublicos =
        BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

    // Agregar aquí únicamente eventos con un acuerdo de publicación entre módulos.
    private static readonly HashSet<string> EventosExpuestos = new(StringComparer.Ordinal);

    private readonly HashSet<string> _tiposExpuestos = new(StringComparer.Ordinal);
    private readonly HashSet<string> _observaciones = new(StringComparer.Ordinal);

    internal ContratosLocales(IEnumerable<Type> tipos)
    {
        var tiposPublicos = tipos.Where(tipo => tipo.IsVisible).ToArray();
        foreach (var contrato in tiposPublicos.Where(EsContratoLocal))
        {
            _tiposExpuestos.Add(contrato.FullName!);
            var visitados = new HashSet<Type>();
            foreach (var interfaz in contrato.GetInterfaces().Prepend(contrato))
            {
                RevisarMiembros(contrato, interfaz, visitados);
            }
        }

        foreach (var evento in tiposPublicos.Where(tipo =>
                     EnNamespace(RelativoAlModulo(tipo.Namespace), "Interfaz.Eventos.OUT") &&
                     !EventosExpuestos.Contains(tipo.FullName!)))
        {
            _observaciones.Add($"Confirmar la exposición del evento {evento.FullName}.");
        }
    }

    internal bool EstaExpuesto(string nombreCompleto) =>
        _tiposExpuestos.Contains(nombreCompleto) || EventosExpuestos.Contains(nombreCompleto);

    internal IEnumerable<string> Observaciones =>
        _observaciones.Order(StringComparer.Ordinal);

    private void RevisarMiembros(Type contrato, Type tipo, HashSet<Type> visitados)
    {
        foreach (var metodo in tipo.GetMethods(MiembrosPublicos))
        {
            RevisarTipo(contrato, metodo, metodo.ReturnType, visitados);
            foreach (var parametro in metodo.GetParameters())
            {
                RevisarTipo(contrato, metodo, parametro.ParameterType, visitados);
            }
        }

        foreach (var campo in tipo.GetFields(MiembrosPublicos))
        {
            RevisarTipo(contrato, campo, campo.FieldType, visitados);
        }

        foreach (var constructor in tipo.GetConstructors(MiembrosPublicos))
        {
            foreach (var parametro in constructor.GetParameters())
            {
                RevisarTipo(contrato, constructor, parametro.ParameterType, visitados);
            }
        }
    }

    private void RevisarTipo(Type contrato, MemberInfo miembro, Type tipo, HashSet<Type> visitados)
    {
        if (tipo.HasElementType)
        {
            RevisarTipo(contrato, miembro, tipo.GetElementType()!, visitados);
            return;
        }

        foreach (var argumento in tipo.GenericTypeArguments)
        {
            RevisarTipo(contrato, miembro, argumento, visitados);
        }

        var modulo = Modulo(tipo.Namespace);
        if (modulo is null || tipo.IsGenericParameter)
        {
            return;
        }

        var relativo = RelativoAlModulo(tipo.Namespace);
        var esDto = tipo.IsVisible && !tipo.IsInterface && !tipo.IsEnum &&
            !tipo.GetInterfaces().Any(EsContratoLocal) &&
            !typeof(Exception).IsAssignableFrom(tipo) &&
            !typeof(Microsoft.EntityFrameworkCore.DbContext).IsAssignableFrom(tipo) &&
            (EnNamespace(relativo, "Aplicacion.DTOs") ||
             EnNamespace(relativo, "Interfaz.Apis.Local.Contratos"));
        if (esDto && modulo == Modulo(contrato.Namespace))
        {
            var definicion = tipo.IsGenericType ? tipo.GetGenericTypeDefinition() : tipo;
            _tiposExpuestos.Add(definicion.FullName!);
            if (visitados.Add(definicion))
            {
                RevisarMiembros(contrato, tipo, visitados);
            }
            return;
        }

        if (!EsContratoLocal(tipo) && !EventosExpuestos.Contains(tipo.FullName!))
        {
            _observaciones.Add(
                $"{contrato.FullName}: {miembro.DeclaringType?.FullName}.{miembro.Name} expone " +
                $"{tipo.FullName}. Revisar su exposición como parte del contrato.");
        }
    }

    private static bool EsContratoLocal(Type tipo) =>
        tipo.IsInterface && tipo.IsVisible &&
        EnNamespace(RelativoAlModulo(tipo.Namespace), "Interfaz.Apis.Local.Contratos");

    internal static string? Modulo(string? espacio) =>
        espacio?.StartsWith(PrefijoModulos, StringComparison.Ordinal) == true
            ? espacio[PrefijoModulos.Length..].Split('.', 2)[0]
            : null;

    internal static string RelativoAlModulo(string? espacio) =>
        Modulo(espacio) is { } modulo && espacio!.Length > PrefijoModulos.Length + modulo.Length
            ? espacio[(PrefijoModulos.Length + modulo.Length + 1)..]
            : "";

    internal static bool EnNamespace(string? espacio, string raiz) =>
        espacio == raiz || espacio?.StartsWith(raiz + ".", StringComparison.Ordinal) == true;
}
