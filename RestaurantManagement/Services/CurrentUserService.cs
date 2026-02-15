using RestaurantManagement.Models;

namespace RestaurantManagement.Services;

public class CurrentUserService
{
    private static CurrentUserService? _instance;
    private static readonly object _lock = new();
    
    public User? CurrentUser { get; private set; }
    
    // Event for notifying when the user changes
    public event EventHandler UserChanged;

    public static CurrentUserService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new CurrentUserService();
                }
            }
            return _instance;
        }
    }

    private CurrentUserService() { }

    public void SetCurrentUser(User user)
    {
        CurrentUser = user;
        OnUserChanged();
    }

    public void LogOut()
    {
        CurrentUser = null;
        OnUserChanged();
    }
    
    // Method to raise the UserChanged event
    protected virtual void OnUserChanged()
    {
        UserChanged?.Invoke(this, EventArgs.Empty);
    }
} 