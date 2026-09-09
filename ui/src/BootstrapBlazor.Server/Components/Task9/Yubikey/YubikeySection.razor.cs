using BootstrapBlazor.Server.Data;
using BootstrapBlazor.Server.Services;
using Microsoft.AspNetCore.Components;

namespace BootstrapBlazor.Server.Components.Task9.Yubikey;

public partial class YubikeySection : ComponentBase
{
    [Inject] private IYubikeyApiService YubikeyApi { get; set; } = default!;

    /// <summary>
    /// How many keys this card is showing, reported upward whenever it changes. The profile page
    /// adds it to the passkey and authenticator counts to warn people down to one way in — this
    /// card owns the YubiKey number, so it reports rather than being queried twice.
    /// </summary>
    [Parameter] public EventCallback<int> OnKeysCounted { get; set; }

    private bool FeatureEnabled { get; set; }
    private bool IsLoading { get; set; } = true;
    private bool IsBusy { get; set; }
    private string? LoadError { get; set; }
    private string? Message { get; set; }
    private bool MessageIsError { get; set; }

    private List<YubikeyDto> Keys { get; set; } = [];
    private bool HasPin => Keys.Any(k => k.HasPin);

    private string NewPin { get; set; } = string.Empty;
    private string ConfirmPin { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var status = await YubikeyApi.GetFeatureStatusAsync();
            FeatureEnabled = status?.Enabled ?? false;
        }
        catch
        {
            // The switch being unreadable means the feature is not usable; hiding the card is the
            // honest outcome, and the login path reports its own failures.
            FeatureEnabled = false;
        }

        if (FeatureEnabled)
        {
            await LoadAsync();
        }
        else
        {
            IsLoading = false;
        }
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        LoadError = null;
        StateHasChanged();

        try
        {
            Keys = await YubikeyApi.GetKeysAsync() ?? [];
        }
        catch (Exception ex)
        {
            Keys = [];
            LoadError = ApiErrorMessage.Describe(ex);
        }
        finally
        {
            // Enrollment arrives here from YubikeyPrompt's [JSInvokable], which re-renders the
            // prompt but not this component. Without this the spinner above stays up until F5.
            IsLoading = false;
            StateHasChanged();
        }

        await OnKeysCounted.InvokeAsync(Keys.Count);
    }

    private async Task SavePinAsync()
    {
        IsBusy = true;
        Message = null;

        try
        {
            await YubikeyApi.SetPinAsync(NewPin, ConfirmPin);
            NewPin = ConfirmPin = string.Empty;
            Message = "Đã lưu mã PIN.";
            MessageIsError = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Message = ApiErrorMessage.Describe(ex);
            MessageIsError = true;
        }
        finally
        {
            IsBusy = false;
            StateHasChanged();
        }
    }

    /// <summary>Returns the message to show, or null on success — the prompt component's contract.</summary>
    private async Task<string?> EnrollAsync(string otp)
    {
        try
        {
            await YubikeyApi.EnrollAsync(otp);
            Message = "Đã đăng ký khoá.";
            MessageIsError = false;
            await LoadAsync();
            return null;
        }
        catch (Exception ex)
        {
            return ApiErrorMessage.Describe(ex);
        }
    }

    private async Task DeleteAsync(YubikeyDto key)
    {
        IsBusy = true;
        Message = null;

        try
        {
            await YubikeyApi.DeleteKeyAsync(key.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Message = ApiErrorMessage.Describe(ex);
            MessageIsError = true;
        }
        finally
        {
            IsBusy = false;
            StateHasChanged();
        }
    }

}
