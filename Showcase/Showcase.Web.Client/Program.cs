using FaFa.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Showcase.Web.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ApiBaseUrl comes from wwwroot/appsettings.json / appsettings.Development.json —
// the showcase's Web.Api project, not the Blazor library (which has no server
// component of its own). Points at Web.Api's launchSettings.json https port by
// default; change it there if you run Web.Api on a different port.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// The one FaFa service that needs registering — see Blazor/docs/fa-toast.md.
builder.Services.AddScoped<FaToastService>();

await builder.Build().RunAsync();
