using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Contracts.Users;

namespace UI.Services;

public class UsersApiClient(HttpClient http, AuthState authState)
{
    private readonly HttpClient _http = http;
    private readonly AuthState _authState = authState;

    public async Task<IReadOnlyList<UserListResponse>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = _authState.CurrentUser;
        if (currentUser is null || string.IsNullOrWhiteSpace(currentUser.Token))
        {
            return Array.Empty<UserListResponse>();
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/users?requireTotalCount=false");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", currentUser.Token);

        using var response = await _http.SendAsync(request, cancellationToken);

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException("You are not authorised to view the users list.");
        }

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<UsersLoadEnvelope>(cancellationToken: cancellationToken);

        return payload?.Data ?? Array.Empty<UserListResponse>();
    }

    private sealed class UsersLoadEnvelope
    {
        [JsonPropertyName("data")]
        public List<UserListResponse> Data { get; set; } = [];
    }
}
