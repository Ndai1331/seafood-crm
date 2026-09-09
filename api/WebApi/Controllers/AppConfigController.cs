using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.AppConfigs;
using Contract.AppConfigs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    
    [ApiController]
    [Route("api/app-config/")]
    [Authorize]
    public class AppConfigController
    {
        private readonly AppConfigService _appConfigService;
        public AppConfigController(AppConfigService appConfigService)
        {
            _appConfigService = appConfigService;
        }
      
        [HttpGet]
        [Route("get-applied-config")]
        public async Task<AppConfigDto> GetAppliedConfigAsync()
        {
            return await _appConfigService.GetAppliedConfigAsync();
        }

        [HttpPut("{id}")]
        public async Task<AppConfigDto> UpdateConfigAsync(int id, AppConfigDto appConfigDto)
        {
            return await _appConfigService.UpdateConfigAsync(appConfigDto);
        }
    }
}