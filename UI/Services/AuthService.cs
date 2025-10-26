using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Contracts.Account;

namespace UI.Services;

public sealed class AuthService(HttpClient httpClient, AuthState authState)
{
    public async Task<(bool Success, string? Error)> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var response = await httpClient.PostAsJsonAsync("api/account/login", request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return (false, "Invalid email or password");

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);

        if (loginResponse is null)
            return (false, "The server returned an unexpected response.");

        authState.SetUser(loginResponse);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);

        return (true, null);
    }

    public void Logout()
    {
        httpClient.DefaultRequestHeaders.Authorization = null;
        authState.Clear();
    }
}
