using Blazored.LocalStorage;
using LegalConnect.Client;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ── Local Storage ────────────────────────────────────────────────────────────
builder.Services.AddBlazoredLocalStorage();

// ── Authentication ───────────────────────────────────────────────────────────
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

// ── HTTP Message Handler (attaches Bearer token) ─────────────────────────────
builder.Services.AddScoped<AuthorizationMessageHandler>();

// ── Named HttpClient: public (no auth) ───────────────────────────────────────
builder.Services.AddHttpClient("public", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]
        ?? builder.HostEnvironment.BaseAddress + "api/");
});

// ── Named HttpClient: secured (with Bearer token) ────────────────────────────
builder.Services.AddHttpClient("secured", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]
        ?? builder.HostEnvironment.BaseAddress + "api/");
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

// ── Application Services ─────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILawyerService, LawyerService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ILawyerClientService, LawyerClientService>();
builder.Services.AddScoped<ICaseService, CaseService>();
builder.Services.AddScoped<IClientLawyerRequestService, ClientLawyerRequestService>();
builder.Services.AddScoped<IDealService, DealService>();
builder.Services.AddScoped<IClientProfileService, ClientProfileService>();
builder.Services.AddScoped<ToastService>();

await builder.Build().RunAsync();
