namespace BootstrapBlazor.Server.Http;

internal sealed class RequestClientCircuitGate : IDisposable
{
    public SemaphoreSlim Semaphore { get; } = new(1, 1);

    public void Dispose() => Semaphore.Dispose();
}
