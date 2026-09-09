using BootstrapBlazor.Server.Http;
using Blazored.LocalStorage;

namespace BootstrapBlazor.Server.Services
{
    public interface IPositionService
    {
        Task<List<PositionDto>> GetListAsync();
    }

    public class PositionService : IPositionService
    {
        private readonly ILocalStorageService _localStorage;

        public PositionService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<List<PositionDto>> GetListAsync()
        {
            await EnsureRequestAuthAsync();
            return await RequestClient.GetAPIAsync<List<PositionDto>>("position");
        }

        private async Task EnsureRequestAuthAsync()
        {
            RequestClient.InjectServices(_localStorage);
            var token = await _localStorage.GetItemAsync<string>("my-access-token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                RequestClient.AttachToken(token);
            }
        }
    }
}
