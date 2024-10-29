using CoindeskAPI.Models;
using CoindeskAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoindeskAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly CoindeskService m_coindeskService;
        private readonly ApplicationDbContext m_context;

        public CurrencyController(CoindeskService coindeskService, ApplicationDbContext context)
        {
            m_coindeskService = coindeskService;
            m_context = context;
        }

        // Existing method to fetch Coindesk data
        [HttpGet("coindesk")]
        public async Task<IActionResult> GetCoindeskData()
        {
            try
            {
                var _result = await m_coindeskService.GetBitcoinPriceIndexAsyncRaw();
                return Ok(_result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Get latest Coindesk data with additional information
        [HttpGet("coindesk/new")]
        public async Task<IActionResult> GetCoindeskNewData([FromQuery] string language = "en-US")
        {
            try
            {
                var _result = await m_coindeskService.GetBitcoinPriceIndexAsync();
                var _timeUpdated = DateTime.Parse(_result["time"]["updatedISO"].ToString()).ToString("yyyy/MM/dd HH:mm:ss");
                var _bpi = _result["bpi"];

                var _response = new
                {
                    UpdatedTime = _timeUpdated,
                    Currencies = _bpi.Children<JProperty>().Select(
                        x => ParseBPI(_bpi, x.Name, language)
                    ).ToList()
                };

                return Ok(_response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        Object ParseBPI(JToken data, string code, string language)
        {
            return new
            {
                Code = data[code]["code"].ToString(),
                Name = m_context.Currencies.FirstOrDefault(c => c.Code == code && c.Language == language)?.Name ?? data[code]["description"].ToString(),
                Rate = data[code]["rate"].ToString()
            };
        }

        [HttpGet("currencies")]
        public async Task<ActionResult<IEnumerable<Currency>>> GetCurrencies()
        {
            return await m_context.Currencies.OrderBy(c => c.Code).ToListAsync();
        }

        [HttpGet("codes")]
        public async Task<ActionResult<IEnumerable<string>>> GetAllCodes()
        {
            var _codes = await m_context.Currencies
                .Select(c => c.Code)
                .Distinct()
                .ToListAsync();

            return _codes;
        }

        [HttpGet("languages")]
        public async Task<ActionResult<IEnumerable<string>>> GetAllLanguages()
        {
            var _languages = await m_context.Currencies
                .Select(c => c.Language)
                .Distinct()
                .ToListAsync();

            return _languages;
        }

        [HttpGet("currency")]
        public async Task<ActionResult<Currency>> GetCurrency([FromQuery] string code, [FromQuery] string language)
        {
            var _currency = await m_context.Currencies.FindAsync(code, language);
            if (_currency == null)
            {
                return NotFound();
            }

            return _currency;
        }

        [HttpPut("currency")]
        public async Task<ActionResult<Currency>> AddCurrency(Currency currency)
        {
            m_context.Currencies.Add(currency);
            await m_context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCurrency), new { code = currency.Code, language = currency.Language }, currency);
        }

        [HttpPatch("currency")]
        public async Task<ActionResult<Currency>> UpdateCurrency([FromBody] Currency currencyUpdates)
        {
            var _existingCurrency = await m_context.Currencies.FindAsync(currencyUpdates.Code, currencyUpdates.Language);
            if (_existingCurrency == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(currencyUpdates.Name))
            {
                _existingCurrency.Name = currencyUpdates.Name;
            }

            try
            {
                await m_context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(500, $"There was an error updating the currency: {ex.Message}");
            }

            return _existingCurrency;
        }

        [HttpDelete("currency")]
        public async Task<IActionResult> DeleteCurrency([FromQuery] string code, [FromQuery] string language)
        {
            var _currency = await m_context.Currencies.FindAsync(code, language);
            if (_currency == null)
            {
                return NotFound();
            }

            m_context.Currencies.Remove(_currency);
            await m_context.SaveChangesAsync();
            var _response = new { message = "seccussfull" };
            return Ok(_response);
        }
    }
}