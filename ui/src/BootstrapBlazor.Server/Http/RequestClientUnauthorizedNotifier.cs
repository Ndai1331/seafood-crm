namespace BootstrapBlazor.Server.Http;

internal sealed class RequestClientUnauthorizedNotifier
{
    public event Action? Detected;

    public void Notify() => Detected?.Invoke();
}
