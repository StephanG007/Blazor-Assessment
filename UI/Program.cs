using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using UI;
using UI.Services;
using UI.Theming;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddMudServices(options =>
{
    options.SnackbarConfiguration.PreventDuplicates = true;
});
builder.Services.AddScoped<AuthState>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<BookingApiClient>();

await builder.Build().RunAsync();
