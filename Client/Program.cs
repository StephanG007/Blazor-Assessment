using System.Net.Http.Headers;
using Client;
using Client.Configuration;
using Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Options;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("Api"));

builder.Services.AddMudServices();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddHttpClient<BookingApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ApiOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        throw new InvalidOperationException("API base URL is not configured.");
    }

    var baseUri = new Uri(options.BaseUrl, UriKind.Absolute);
    if (!string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("The API base URL must use HTTPS.");
    }

    client.BaseAddress = baseUri;
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

await builder.Build().RunAsync();
