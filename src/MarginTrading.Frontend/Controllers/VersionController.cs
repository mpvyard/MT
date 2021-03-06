﻿using System.Threading.Tasks;
using MarginTrading.Common.ClientContracts;
using MarginTrading.Core;
using MarginTrading.Frontend.Services;
using MarginTrading.Frontend.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;
using IsAliveResponse = MarginTrading.Common.BackendContracts.IsAliveResponse;

namespace MarginTrading.Frontend.Controllers
{
    [Route("api/isAlive")]
    public class VersionController : Controller
    {
        private readonly MtFrontendSettings _setings;
        private readonly IHttpRequestService _httpRequestService;
        private readonly WampSessionsService _wampSessionsService;
        private readonly IDateService _dateService;

        public VersionController(MtFrontendSettings setings,
            IHttpRequestService httpRequestService,
            WampSessionsService wampSessionsService,
            IDateService dateService)
        {
            _setings = setings;
            _httpRequestService = httpRequestService;
            _wampSessionsService = wampSessionsService;
            _dateService = dateService;
        }

        [HttpGet]
        public async Task<IsAliveExtendedResponse> Get()
        {
            var result = new IsAliveExtendedResponse
            {
                Version = PlatformServices.Default.Application.ApplicationVersion,
                Env = _setings.MarginTradingFront.Env,
                ServerTime = _dateService.Now()
            };

            try
            {
                var responce = await _httpRequestService.GetAsync<IsAliveResponse>("isAlive");
                result.LiveVersion = responce.Version;
            }
            catch
            {
                result.LiveVersion = "Error";
            }

            try
            {
                var responce = await _httpRequestService.GetAsync<IsAliveResponse>("isAlive", false);
                result.DemoVersion = responce.Version;
            }
            catch
            {
                result.DemoVersion = "Error";
            }

            result.WampOpened = _wampSessionsService.OpenedSessionsCount;

            return result;
        }
    }
}
