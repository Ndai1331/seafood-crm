using Contract.MenuLayout;
using Core.Const;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;

namespace WebApi.Controllers
{
    // Read is plain [Authorize] — every user's menu build needs it and order data isn't sensitive
    // (leaks nothing beyond page Text already visible to that user). Write is permission-gated:
    // this is presentation state, not security state, but the mutation itself still needs a gate.
    [ApiController]
    [Route("api/menu-layout")]
    [Authorize]
    public class MenuLayoutController : ControllerBase
    {
        private readonly IMenuLayoutService _menuLayoutService;

        public MenuLayoutController(IMenuLayoutService menuLayoutService)
        {
            _menuLayoutService = menuLayoutService;
        }

        [HttpGet]
        public async Task<List<MenuOverrideDto>> GetAsync()
        {
            return await _menuLayoutService.GetAsync();
        }

        [HttpPost]
        [HasPermission(Permissions.MenuArrangement)]
        public async Task SaveAsync(MenuLayoutSnapshotDto input)
        {
            await _menuLayoutService.SaveAsync(input, User.Identity?.Name);
        }
    }
}
