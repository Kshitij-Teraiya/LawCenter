using Blazored.LocalStorage;

namespace LegalConnect.Client.Helpers;

/// <summary>
/// DelegatingHandler that automatically attaches the stored JWT as a Bearer
/// Authorization header on every outgoing <see cref="HttpRequestMessage"/>.
/// </summary>
public class AuthorizationMessageHandler : DelegatingHandler
{
    private const string TokenKey = "jwt_token";
    private readonly ILocalStorageService _localStorage;

    public AuthorizationMessageHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _localStorage.GetItemAsStringAsync(TokenKey);

        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
