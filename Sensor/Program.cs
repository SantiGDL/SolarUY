using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
Radzen.ServiceCollectionExtensions.AddRadzenComponents(builder.Services);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddTransient<Sensor.Servicios.MedicionesCliente>();
builder.Services.AddTransient<Sensor.Servicios.SimulacionMedicionesService>();

await builder.Build().RunAsync();
