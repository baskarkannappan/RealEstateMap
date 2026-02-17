using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RealEstateMap;
using RealEstateMap.Models;
using RealEstateMap.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("Api"));

builder.Services.AddHttpClient("ApiClient", (sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<ApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(20);
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHouseService, HouseService>();

await builder.Build().RunAsync();
