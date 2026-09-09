namespace BootstrapBlazor.Server.Services.I18n;

public interface IAppLang
{
    string this[string code] { get; }
    string Current { get; }
    Task SetLangAsync(string lang);
}
