using Contracts.Account;

namespace UI.Services;

public sealed class AuthState
{
    private LoginResponse? _currentUser;

    public LoginResponse? CurrentUser
    {
        get => _currentUser;
        private set
        {
            _currentUser = value;
            NotifyStateChanged();
        }
    }

    public event Action? StateChanged;

    public void SetUser(LoginResponse? user)
    {
        CurrentUser = user;
    }

    public void Clear()
    {
        CurrentUser = null;
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
