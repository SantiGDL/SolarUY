using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SolarUy.Backend.Infraestructura.Persistencia;
using SolarUy.Backend.Modulos.Consultas.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Consultas.Aplicacion.Implementaciones;
using SolarUy.Backend.Modulos.Consultas.Infraestructura.IA;
using SolarUy.Backend.Modulos.Mediciones.Aplicacion.Implementaciones;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Historial;
using SolarUy.Backend.Modulos.Mediciones.Dominio.Repositorios;
using SolarUy.Backend.Modulos.Mediciones.Infraestructura.Persistencia;
using SolarUy.Backend.Modulos.Mediciones.Infraestructura.Repositorios;
using SolarUy.Backend.Modulos.Mediciones.Interfaz.SignalR;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Implementaciones;
using SolarUy.Backend.Modulos.Parques.Dominio.Repositorios;
using SolarUy.Backend.Modulos.Parques.Infraestructura.Repositorios;
using SolarUy.Backend.Modulos.Parques.Interfaz.Apis.Local.Contratos;
using SolarUy.Backend.Modulos.Tareas.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Tareas.Aplicacion.Implementaciones;
using SolarUy.Backend.Modulos.Tareas.Infraestructura.Repositorios;
using SolarUy.Components;
using SolarUy.Components.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "SolarUy.Session";
        options.Cookie.HttpOnly = true;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"]
            ?? throw new InvalidOperationException("Falta configurar Keycloak:Authority.");
        options.Audience = builder.Configuration["Keycloak:Audience"] ?? "account";
        options.RequireHttpsMetadata =
            builder.Configuration.GetValue(
                "Keycloak:RequireHttpsMetadata",
                !builder.Environment.IsDevelopment());
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            NameClaimType = "preferred_username",
            RoleClaimType = "roles",
            ValidateLifetime = true
        };
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"]
            ?? throw new InvalidOperationException("Falta configurar Keycloak:Authority.");
        options.ClientId = builder.Configuration["Keycloak:ClientId"]
            ?? throw new InvalidOperationException("Falta configurar Keycloak:ClientId.");
        options.RequireHttpsMetadata =
            builder.Configuration.GetValue(
                "Keycloak:RequireHttpsMetadata",
                !builder.Environment.IsDevelopment());
        options.ResponseType = "code";
        options.UsePkce = true;
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.MapInboundClaims = false;
        options.TokenValidationParameters.NameClaimType = "preferred_username";
        options.TokenValidationParameters.RoleClaimType = "roles";
        options.Scope.Add("profile");
        options.Scope.Add("email");
    });
builder.Services.AddAuthorization();
builder.Services.AddSignalR();

var connectionString = builder.Configuration.GetConnectionString("SolarUy")
    ?? throw new InvalidOperationException(
        "Falta ConnectionStrings:SolarUy. Configúrala mediante User Secrets o una variable de entorno.");

builder.Services.AddDbContextFactory<ParquesDbContext>(options =>
    options.UseMySQL(connectionString));
builder.Services.AddScoped<IParqueRepositorio, ParqueRepositorio>();
builder.Services.AddScoped<INodoRepositorio, NodoRepositorio>();
builder.Services.AddScoped<IPanelRepositorio, PanelRepositorio>();
builder.Services.AddScoped<ITareaRepositorio, TareaRepositorio>();
builder.Services.AddScoped<IParquesService, ParquesService>();
builder.Services.AddScoped<INodosService, NodosService>();
builder.Services.AddScoped<IPanelesService, PanelService>();
builder.Services.AddScoped<ITareasService, TareasService>();
builder.Services.AddScoped<IUsuarioActual, UsuarioActual>();
builder.Services.AddDbContextFactory<MedicionesDbContext>(options =>
    options.UseMySQL(connectionString, mysql => mysql.MigrationsHistoryTable("__MedicionesMigrationsHistory")));
builder.Services.AddScoped<IMedicionRepository, MedicionRepository>();
builder.Services.AddScoped<ICatalogoProduccion, CatalogoProduccionService>();
builder.Services.AddScoped<MedicionesService>();
builder.Services.AddScoped<ICatalogoHistorial, CatalogoHistorialService>();
builder.Services.AddScoped<SolarUy.Backend.Modulos.Mediciones.Aplicacion.Contratos.IAccesoHistorial, AccesoHistorialActual>();
builder.Services.AddScoped<HistorialMedicionesService>();
builder.Services.AddScoped<ConsultaNaturalService>();
builder.Services.AddHttpClient<IProveedorConsultaIA, ProveedorConsultaLocal>((_, client) =>
{
    client.BaseAddress = new Uri(builder.Configuration["Consultas:OllamaUrl"] ?? builder.Configuration["IA:OllamaUrl"] ?? "http://localhost:11434/");
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddScoped<SolarUy.Backend.Modulos.Mediciones.Aplicacion.Contratos.IExportadorHistorialPdf,
    SolarUy.Backend.Modulos.Mediciones.Infraestructura.Exportacion.ExportadorHistorialPdf>();
SolarUy.Backend.Modulos.Mediciones.Infraestructura.Exportacion.FuentesHistorial.Configurar();
builder.Services.AddSingleton<SolarUy.Backend.Modulos.Mediciones.Aplicacion.Contratos.ISesionesProduccion,
    SolarUy.Backend.Modulos.Mediciones.Infraestructura.Sesiones.SesionesProduccion>();
builder.Services.AddScoped<SolarUy.Backend.Modulos.Mediciones.Aplicacion.Contratos.IControlMotor,
    SolarUy.Backend.Modulos.Mediciones.Infraestructura.SignalR.ControlMotor>();
Radzen.ServiceCollectionExtensions.AddRadzenComponents(builder.Services);
builder.Services.AddHostedService<SolarUy.Backend.Modulos.Mediciones.Infraestructura.Sesiones.LimpiezaSesionesService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapGet("/login", (string? returnUrl) =>
{
    var destino = EsRutaLocal(returnUrl) ? returnUrl! : "/admin";
    return Results.Challenge(
        new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            RedirectUri = destino
        },
        [OpenIdConnectDefaults.AuthenticationScheme]);
}).AllowAnonymous();

app.MapPost("/logout", async (HttpContext context, IAntiforgery antiforgery) =>
{
    await antiforgery.ValidateRequestAsync(context);
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignOutAsync(
        OpenIdConnectDefaults.AuthenticationScheme,
        new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            RedirectUri = "/"
        });
});

app.MapPost("/mcp", async (HttpContext context, ConsultaNaturalService consultas) =>
{
    if (context.Request.Body is null)
        return Results.BadRequest(new { jsonrpc = "2.0", error = new { code = -32600, message = "Falta el cuerpo JSON." } });

    using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
    var json = await reader.ReadToEndAsync(context.RequestAborted);
    if (string.IsNullOrWhiteSpace(json))
        return Results.BadRequest(new { jsonrpc = "2.0", error = new { code = -32600, message = "El cuerpo de la solicitud no puede estar vacío." } });

    using var document = JsonDocument.Parse(json);
    var root = document.RootElement;
    var method = root.TryGetProperty("method", out var methodElement) ? methodElement.GetString() ?? string.Empty : string.Empty;
    var id = root.TryGetProperty("id", out var idElement) ? idElement.GetRawText() : null;

    if (string.Equals(method, "tools/list", StringComparison.OrdinalIgnoreCase))
    {
        return Results.Json(new
        {
            jsonrpc = "2.0",
            id,
            result = new
            {
                tools = new object[]
                {
                    new
                    {
                        name = "listar_opciones",
                        description = "Lista parques, nodos y paneles con historial disponible.",
                        inputSchema = new { type = "object", properties = new { }, additionalProperties = false }
                    },
                    new
                    {
                        name = "consultar_historial",
                        description = "Consulta mediciones históricas por parque, nodo, panel y periodo.",
                        inputSchema = new
                        {
                            type = "object",
                            properties = new
                            {
                                parqueId = new { type = "integer", description = "Identificador del parque." },
                                nodoId = new { type = "integer", description = "Identificador del nodo, opcional." },
                                panelId = new { type = "integer", description = "Identificador del panel, opcional." },
                                periodo = new { type = "string", @enum = new[] { "Todos", "Ultimas24Horas", "Ultimos30Dias", "Ultimos365Dias" }, description = "Periodo solicitado." },
                                modo = new { type = "string", @enum = new[] { "Diario", "Promedio", "Crudos" }, description = "Modo de agregación." }
                            },
                            required = new[] { "parqueId" },
                            additionalProperties = false
                        }
                    }
                }
            }
        });
    }

    if (string.Equals(method, "tools/call", StringComparison.OrdinalIgnoreCase))
    {
        var parametros = root.GetProperty("params");
        var toolName = parametros.GetProperty("name").GetString() ?? string.Empty;
        var argumentos = parametros.TryGetProperty("arguments", out var argumentosElemento) ? argumentosElemento : JsonDocument.Parse("{}").RootElement;

        string respuesta;
        if (toolName == "listar_opciones")
        {
            respuesta = await consultas.ListarOpcionesAsync(context.RequestAborted);
        }
        else if (toolName == "consultar_historial")
        {
            if (!argumentos.TryGetProperty("parqueId", out var parqueIdElement) || !parqueIdElement.TryGetInt32(out var parqueId))
                throw new InvalidOperationException("Hace falta el campo parqueId.");

            int? nodoId = null;
            if (argumentos.TryGetProperty("nodoId", out var nodoIdElement) && nodoIdElement.TryGetInt32(out var nodoIdValor))
                nodoId = nodoIdValor;

            int? panelId = null;
            if (argumentos.TryGetProperty("panelId", out var panelIdElement) && panelIdElement.TryGetInt32(out var panelIdValor))
                panelId = panelIdValor;

            var periodo = ParsePeriodo(argumentos.TryGetProperty("periodo", out var periodoElement)
                ? periodoElement.GetString()
                : null);
            var modo = ParseModo(argumentos.TryGetProperty("modo", out var modoElement)
                ? modoElement.GetString()
                : null);

            respuesta = await consultas.ConsultarPorFiltrosAsync(parqueId, nodoId, panelId, periodo, modo, context.RequestAborted);
        }
        else
        {
            throw new InvalidOperationException($"La herramienta '{toolName}' no existe.");
        }

        return Results.Json(new
        {
            jsonrpc = "2.0",
            id,
            result = new
            {
                content = new[] { new { type = "text", text = respuesta } },
                isError = false
            }
        });
    }

    return Results.Json(new { jsonrpc = "2.0", id, error = new { code = -32601, message = "El método solicitado no está soportado." } });
})
.RequireAuthorization(new AuthorizeAttribute
{
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "ADMIN"
});

app.MapStaticAssets();
app.MapHub<MedicionesHub>("/hubs/mediciones");
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Sensor.Components.MotorSensor).Assembly);

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var contextFactory = scope.ServiceProvider.GetRequiredService<
        IDbContextFactory<ParquesDbContext>>();
    await using var context = await contextFactory.CreateDbContextAsync();
    await context.Database.MigrateAsync();
    var medicionesFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MedicionesDbContext>>();
    await using var medicionesContext = await medicionesFactory.CreateDbContextAsync();
    await medicionesContext.Database.MigrateAsync();
}

app.Run();

static PeriodoHistorial ParsePeriodo(string? valor) => valor switch
{
    "Ultimas24Horas" => PeriodoHistorial.Ultimas24Horas,
    "Ultimos30Dias" => PeriodoHistorial.Ultimos30Dias,
    "Ultimos365Dias" => PeriodoHistorial.Ultimos365Dias,
    _ => PeriodoHistorial.Todos
};

static ModoHistorial ParseModo(string? valor) => valor switch
{
    "Promedio" => ModoHistorial.Promedio,
    "Crudos" => ModoHistorial.Crudos,
    _ => ModoHistorial.Diario
};

static bool EsRutaLocal(string? ruta) =>
    !string.IsNullOrWhiteSpace(ruta)
    && ruta.StartsWith("/", StringComparison.Ordinal)
    && !ruta.StartsWith("//", StringComparison.Ordinal)
    && !ruta.StartsWith("/\\", StringComparison.Ordinal);
