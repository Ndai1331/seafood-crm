using BootstrapBlazor.Server.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BootstrapBlazor.Server.Components.Task9.Yubikey;

public enum PromptStage { Idle, Pin, Touch, Complete }

/// <summary>
/// Drives the PIN and touch steps. Keystroke capture lives in yubikey-capture.js; this class only
/// arms it, receives the finished string, and decides what to do with it.
/// </summary>
public partial class YubikeyPrompt : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    /// <summary>Half-authenticated token when this is a login. Null on the profile page.</summary>
    [Parameter] public string? TemporaryToken { get; set; }

    /// <summary>Whether the PIN step applies. Skipped when the account has no PIN set yet.</summary>
    [Parameter] public bool RequirePin { get; set; } = true;

    /// <summary>
    /// Which step comes first. True after a password: the account is already known, so the PIN can
    /// be asked for and the touch confirms it. False when signing in with no password at all —
    /// there the touch is what names the account, and nobody can be asked for a PIN before it.
    /// </summary>
    [Parameter] public bool PinFirst { get; set; } = true;

    [Parameter] public string IdleLead { get; set; } =
        "Cắm khoá vào máy rồi bấm nút bên dưới.";
    [Parameter] public string IdleAction { get; set; } = "Đăng nhập với YubiKey";

    /// <summary>Handed the captured OTP. Returns an error message, or null when it succeeded.</summary>
    [Parameter] public Func<string, Task<string?>>? OnCaptured { get; set; }

    /// <summary>Verifies the PIN. Returns an error message, or null when it passed.</summary>
    [Parameter] public Func<string, Task<string?>>? OnPinSubmitted { get; set; }

    private PromptStage Stage { get; set; } = PromptStage.Idle;
    private string Pin { get; set; } = string.Empty;
    private string? Error { get; set; }
    private int Captured { get; set; }
    private bool Busy { get; set; }

    private ElementReference _pinInput;
    private IJSObjectReference? _module;
    private DotNetObjectReference<YubikeyPrompt>? _self;
    private bool _shouldFocusPin;

    private string FirstStepTitle =>
        RequirePin && PinFirst ? "Xác minh PIN thành công" : "Sẵn sàng nhận khoá";

    private string SecondStepTitle => Stage switch
    {
        PromptStage.Complete => "Đã nhận khoá",
        _ => "Sẵn sàng — đang chờ YubiKey…"
    };

    private async Task StartAsync()
    {
        Error = null;
        Captured = 0;

        if (RequirePin && PinFirst)
        {
            Stage = PromptStage.Pin;
            _shouldFocusPin = true;
            return;
        }

        await ArmAsync();
    }

    private async Task OnPinKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await SubmitPinAsync();
        }
    }

    private async Task SubmitPinAsync()
    {
        if (Busy || OnPinSubmitted is null) return;

        Busy = true;
        Error = null;
        StateHasChanged();

        try
        {
            Error = await OnPinSubmitted(Pin);
        }
        finally
        {
            Busy = false;
        }

        if (Error is not null)
        {
            return;
        }

        Pin = string.Empty;

        if (PinFirst)
        {
            await ArmAsync();
            return;
        }

        // Passwordless: the touch already happened, so the PIN was the last thing owed.
        Stage = PromptStage.Complete;
        StateHasChanged();
    }

    private async Task ArmAsync()
    {
        Stage = PromptStage.Touch;
        Captured = 0;
        StateHasChanged();

        _module ??= await JS.InvokeAsync<IJSObjectReference>("import", WebAuthnModulePath.YubikeyCapture);
        _self ??= DotNetObjectReference.Create(this);
        await _module.InvokeVoidAsync("arm", _self, 60);
    }

    [JSInvokable]
    public Task OnProgress(int count)
    {
        Captured = count;
        StateHasChanged();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public async Task OnKeystrokesCaptured(string otp)
    {
        Captured = otp.Length;

        if (OnCaptured is null) return;

        Busy = true;
        StateHasChanged();

        string? error;
        try
        {
            error = await OnCaptured(otp);
        }
        finally
        {
            Busy = false;
        }

        if (error is null)
        {
            // Passwordless still owes a PIN; the password path has nothing left to ask for.
            if (PinFirst)
            {
                Stage = PromptStage.Complete;
            }
            else
            {
                Stage = PromptStage.Pin;
                _shouldFocusPin = true;
            }

            Error = null;
        }
        else
        {
            // Back to the start rather than leaving the touch step open: the counter would keep
            // showing a number from a run that has already been rejected.
            Stage = PromptStage.Idle;
            Error = error;
        }

        StateHasChanged();
    }

    [JSInvokable]
    public Task OnTimedOut()
    {
        Stage = PromptStage.Idle;
        Error = "Không nhận được tín hiệu từ khoá. Kiểm tra khoá đã cắm chưa rồi thử lại.";
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task CancelAsync()
    {
        await DisarmAsync();
        Stage = PromptStage.Idle;
        Pin = string.Empty;
        Captured = 0;
        Error = null;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_shouldFocusPin)
        {
            _shouldFocusPin = false;
            try
            {
                await _pinInput.FocusAsync();
            }
            catch (JSException)
            {
                // Focus is a courtesy; losing it is not worth breaking the screen over.
            }
        }
    }

    private async Task DisarmAsync()
    {
        if (_module is null) return;
        try
        {
            await _module.InvokeVoidAsync("disarm");
        }
        catch (JSDisconnectedException)
        {
            // The circuit went away; the listener went with it.
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisarmAsync();

        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException) { }
        }

        _self?.Dispose();
    }
}
