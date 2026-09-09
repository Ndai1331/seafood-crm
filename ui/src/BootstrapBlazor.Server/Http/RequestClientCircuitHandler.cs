using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BootstrapBlazor.Server.Http;

internal sealed class RequestClientCircuitHandler(
    ILocalStorageService localStorage,
    RequestClientUnauthorizedNotifier unauthorizedNotifier,
    RequestClientCircuitGate circuitGate) : CircuitHandler
{
    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> next) =>
        context => RequestClient.RunWithServicesAsync(
            localStorage,
            unauthorizedNotifier,
            circuitGate,
            () => next(context));
}
