namespace LegalConnect.Client.Services;

public enum ToastType { Success, Error, Info, Warning }

public record ToastMessage(Guid Id, string Message, ToastType Type, int DurationMs = 4000);

/// <summary>
/// Simple in-memory toast notification bus.
/// Components subscribe to <see cref="OnChange"/> and render the toast list.
/// </summary>
public class ToastService
{
    private readonly List<ToastMessage> _toasts = [];

    public IReadOnlyList<ToastMessage> Toasts => _toasts.AsReadOnly();

    public event Action? OnChange;

    public void ShowSuccess(string message, int durationMs = 4000)
        => Add(message, ToastType.Success, durationMs);

    public void ShowError(string message, int durationMs = 5000)
        => Add(message, ToastType.Error, durationMs);

    public void ShowInfo(string message, int durationMs = 4000)
        => Add(message, ToastType.Info, durationMs);

    public void ShowWarning(string message, int durationMs = 4000)
        => Add(message, ToastType.Warning, durationMs);

    public void Remove(Guid id)
    {
        _toasts.RemoveAll(t => t.Id == id);
        OnChange?.Invoke();
    }

    private void Add(string message, ToastType type, int durationMs)
    {
        var toast = new ToastMessage(Guid.NewGuid(), message, type, durationMs);
        _toasts.Add(toast);
        OnChange?.Invoke();

        // Auto-remove after duration
        _ = Task.Delay(durationMs).ContinueWith(_ =>
        {
            _toasts.RemoveAll(t => t.Id == toast.Id);
            OnChange?.Invoke();
        });
    }
}
