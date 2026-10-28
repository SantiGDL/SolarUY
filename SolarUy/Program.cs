using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using SolarUy.Components;
using SolarUy.Backend.Infraestructura.Persistencia;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Contratos;
using SolarUy.Backend.Modulos.Parques.Aplicacion.Implementaciones;
using SolarUy.Backend.Modulos.Parques.Dominio.Repositorios;
using SolarUy.Backend.Modulos.Parques.Infraestructura.Repositorios;
using SolarUy.Components.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

var connectionString = builder.Configuration.GetConnectionString("SolarUy")
    ?? throw new InvalidOperationException(
        "Falta ConnectionStrings:SolarUy. Configúrala mediante User Secrets o una variable de entorno.");

builder.Services.AddDbContextFactory<ParquesDbContext>(options =>
    options.UseMySQL(connectionString));
builder.Services.AddScoped<IParqueRepositorio, ParqueRepositorio>();
builder.Services.AddScoped<INodoRepositorio, NodoRepositorio>();
builder.Services.AddScoped<IParquesService, ParquesService>();
builder.Services.AddScoped<INodosService, NodosService>();
builder.Services.AddScoped<IUsuarioActual, UsuarioActual>();

var app = builder.Build();

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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var contextFactory = scope.ServiceProvider.GetRequiredService<
        IDbContextFactory<ParquesDbContext>>();
    await using var context = await contextFactory.CreateDbContextAsync();
    await context.Database.MigrateAsync();
}

app.Run();

static bool EsRutaLocal(string? ruta) =>
    !string.IsNullOrWhiteSpace(ruta)
    && ruta.StartsWith("/", StringComparison.Ordinal)
    && !ruta.StartsWith("//", StringComparison.Ordinal)
    && !ruta.StartsWith("/\\", StringComparison.Ordinal);