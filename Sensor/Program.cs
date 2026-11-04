using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddTransient<Sensor.Servicios.MedicionesCliente>();
builder.Services.AddTransient<Sensor.Servicios.SimulacionMedicionesService>();

await builder.Build().RunAsync();
