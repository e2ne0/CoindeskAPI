using CoindeskAPI.Controllers;
using CoindeskAPI.Models;
using CoindeskAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace CoindeskAPI.Tests
{
    public class CurrencyControllerTests
    {
        private readonly CurrencyController m_controller;
        private readonly ApplicationDbContext m_context;

        public CurrencyControllerTests()
        {
            var _httpClient = new HttpClient();
            var _coindeskService = new CoindeskService(_httpClient);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            m_context = new ApplicationDbContext(options);

            m_context.Currencies.Add(new Currency { Code = "USD", Language = "en-US", Name = "US Dollar" });
            m_context.Currencies.Add(new Currency { Code = "GBP", Language = "en-US", Name = "British Pound" });
            m_context.SaveChanges();

            m_controller = new CurrencyController(_coindeskService, m_context);
        }

        [Fact]
        public async Task GetCoindeskData_ReturnsOkResult()
        {
            var _result = await m_controller.GetCoindeskData();

            var _okResult = Assert.IsType<OkObjectResult>(_result);
            Assert.Equal(200, _okResult.StatusCode);
            Assert.NotNull(_okResult.Value);

            var _responseContent = _okResult.Value as string;
            Assert.Contains("USD", _responseContent);
            Assert.Contains("GBP", _responseContent);
            Assert.Contains("EUR", _responseContent);
        }


        [Fact]
        public async Task GetCoindeskNewData_ReturnsUpdatedData()
        {

            var _result = await m_controller.GetCoindeskNewData("en-US");

            var _okResult = Assert.IsType<OkObjectResult>(_result);
            Assert.NotNull(_okResult.Value);

            var _data = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_okResult.Value));
            Assert.NotNull(_data);
            Assert.NotNull(_data["UpdatedTime"].ToString());
            Assert.NotNull(_data["Currencies"][0]["Name"].ToString());
            Assert.NotNull(_data["Currencies"][0]["Rate"].ToString());
        }

        [Fact]
        public async Task GetCurrencies_ReturnsAllCurrencies()
        {
            var _result = await m_controller.GetCurrencies();

            var _okResult = _result.Value;
            Assert.Equal(2, _okResult.Count());
        }

        [Fact]
        public async Task GetCurrency_ReturnsCurrencyWhenFound()
        {
            var _result = await m_controller.GetCurrency("USD", "en-US");

            Assert.IsType<ActionResult<Currency>>(_result);
            var _okResult = _result.Value;
            Assert.Equal("USD", _okResult.Code);
            Assert.Equal("US Dollar", _okResult.Name);
        }

        [Fact]
        public async Task GetCurrency_ReturnsCurrencyCodes()
        {
            var _result = await m_controller.GetAllCodes();

            Assert.IsType<ActionResult<IEnumerable<string>>>(_result);
            var _okResult = _result.Value;
            Assert.Equal(2, _okResult.Count());
        }

        [Fact]
        public async Task GetCurrency_ReturnsCurrencyLanguage()
        {
            var _result = await m_controller.GetAllLanguages();

            Assert.IsType<ActionResult<IEnumerable<string>>>(_result);
            var _okResult = _result.Value;
            Assert.Equal(1, _okResult.Count());
        }

        [Fact]
        public async Task AddCurrency_AddsNewCurrency()
        {
            var _newCurrency = new Currency { Code = "GBP", Language = "en-GB", Name = "British Pound" };

            var _result = await m_controller.AddCurrency(_newCurrency);

            Assert.IsType<CreatedAtActionResult>(_result.Result);
            var _addedCurrency = await m_context.Currencies.FindAsync("GBP", "en-GB");
            Assert.NotNull(_addedCurrency);
            Assert.Equal("British Pound", _addedCurrency.Name);
        }

        [Fact]
        public async Task UpdateCurrency_UpdatesExistingCurrency()
        {
            var _updateData = new Currency { Code = "USD", Language = "en-US", Name = "US Dollar Updated" };

            var _result = await m_controller.UpdateCurrency(_updateData);

            Assert.IsType<ActionResult<Currency>>(_result);
            var _updatedCurrency = await m_context.Currencies.FindAsync("USD", "en-US");
            Assert.Equal("US Dollar Updated", _updatedCurrency.Name);
        }

        [Fact]
        public async Task DeleteCurrency_RemovesCurrency()
        {
            var _result = await m_controller.DeleteCurrency("USD", "en-US");

            var _deletedCurrency = await m_context.Currencies.FindAsync("USD", "en-US");
            Assert.Null(_deletedCurrency);
            var _okResult = Assert.IsType<OkObjectResult>(_result);
            Assert.NotNull(_okResult.Value);

            var _data = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_okResult.Value));
            Assert.Equal("seccussfull", _data["message"].ToString());
        }
    }
}
