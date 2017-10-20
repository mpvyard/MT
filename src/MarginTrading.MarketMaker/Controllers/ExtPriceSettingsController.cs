﻿using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Models.Api;
using MarginTrading.MarketMaker.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;

namespace MarginTrading.MarketMaker.Controllers
{
    [Route("api/[controller]")]
    public class ExtPriceSettingsController : Controller
    {
        private readonly IPriceCalcSettingsService _priceCalcSettingsService;

        public ExtPriceSettingsController(IPriceCalcSettingsService priceCalcSettingsService)
        {
            _priceCalcSettingsService = priceCalcSettingsService;
        }

        /// <summary>
        /// Inserts or updates settings for an asset pair
        /// </summary>
        [HttpPost]
        [Route("set")]
        [SwaggerOperation("SetSettings")]
        public async Task<IActionResult> Set([FromBody] AssetPairExtPriceSettingsModel settings)
        {
            await _priceCalcSettingsService.Set(settings);
            return Ok(new {success = true});
        }

        /// <summary>
        /// Gets all existing settings
        /// </summary>
        [HttpGet]
        [Route("")]
        [SwaggerOperation("GetAllSettings")]
        public Task<IReadOnlyList<AssetPairExtPriceSettingsModel>> GetAll()
        {
            return _priceCalcSettingsService.GetAllAsync();
        }

        /// <summary>
        /// Set settings for a single asset pair
        /// </summary>
        [HttpGet]
        [Route("{assetPairId}")]
        [SwaggerOperation("GetSettings")]
        [CanBeNull]
        public Task<IReadOnlyList<AssetPairExtPriceSettingsModel>> Get(string assetPairId)
        {
            return _priceCalcSettingsService.GetAllAsync(assetPairId);
        }
    }
}