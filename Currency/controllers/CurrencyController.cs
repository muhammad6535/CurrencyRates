using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Currency.models;
using System.Linq;
using Currency.models.Currency.CuurencyDto;
using System.Net;

namespace Currency.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CurrencyController(ApplicationDbContext context)
        {
            _context = context;
        }

        //------------------APIs-------------------
        [HttpGet("CurrenciesRates")]
        public IActionResult GetCrrenciesRates()
        {
            _ = UpdateCurrenciesRates();
            return Ok(_context.CurrenciesRates.ToList());
        }

        [HttpPut("CurrenciesRates")]
        public async Task<IActionResult> UpdateCurrenciesRates()
        {
            try
            {
                var task = await Task.Run(() => GetURI(new Uri("https://v6.exchangerate-api.com/v6/0b40cd2f00393d3b4d06e909/latest/USD")));
                var relevantCurrencies = JsonConvert.DeserializeObject<CurrencyDto>(task);
                UpdateCurrenciesInDB("USD","ILS", relevantCurrencies);
                UpdateCurrenciesInDB("GBP", "EUR", relevantCurrencies);
                UpdateCurrenciesInDB("EUR", "JPY", relevantCurrencies);
                UpdateCurrenciesInDB("EUR", "USD", relevantCurrencies);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }

        [HttpPost("CurrenciesRates")]
        public async Task<IActionResult> InsertIntoDB()
        {
            try
            {
                var task = await Task.Run(() => GetURI(new Uri("https://v6.exchangerate-api.com/v6/0b40cd2f00393d3b4d06e909/latest/USD")));
                var relevantCurrencies = JsonConvert.DeserializeObject<CurrencyDto>(task);
                var lastUpdateDate = DateTime.Parse(relevantCurrencies.time_last_update_utc);
                InsertCurrency("USD", "ILS", relevantCurrencies);
                InsertCurrency("GBP", "EUR", relevantCurrencies);
                InsertCurrency("EUR", "JPY", relevantCurrencies);
                InsertCurrency("EUR", "USD", relevantCurrencies);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }

        //----------------Help Methods-------------------
        //Update Currency In DB
        private void UpdateCurrenciesInDB(string from, string to, CurrencyDto relevantCurrencies)
        {
            var currency = _context.CurrenciesRates.FirstOrDefault(o => o.Name.Contains(from) && o.Name.Contains(to));
            if(currency == null)
            {
                return;
            }
            var valueOfFrom =
            relevantCurrencies.conversion_rates.GetType().GetProperty(from).GetValue(relevantCurrencies.conversion_rates, null).ToString();
            var valueOfTo =
                relevantCurrencies.conversion_rates.GetType().GetProperty(to).GetValue(relevantCurrencies.conversion_rates, null).ToString();
            currency.Value = ConvertCurrency(double.Parse(valueOfFrom), double.Parse(valueOfTo));
            currency.LastUpdateDate = DateTime.Parse(relevantCurrencies.time_last_update_utc);
            _context.SaveChanges();
        }

        //Insert new currency to the DB
        private void InsertCurrency(string from, string to, CurrencyDto relevantCurrencies)
        {
            var currency = _context.CurrenciesRates.FirstOrDefault(o => o.Name.Contains(from) && o.Name.Contains(to));
            if (currency != null)
            {
                return;
            }
            CurrenciesRates currencyRates = new();
            var valueOfFrom =
             relevantCurrencies.conversion_rates.GetType().GetProperty(from).GetValue(relevantCurrencies.conversion_rates, null).ToString();
            var valueOfTo =
                relevantCurrencies.conversion_rates.GetType().GetProperty(to).GetValue(relevantCurrencies.conversion_rates, null).ToString();
            currencyRates.Name = from + "/" + to;
            currencyRates.Value =
                ConvertCurrency(double.Parse(valueOfFrom), double.Parse(valueOfTo));
            currencyRates.LastUpdateDate = DateTime.Parse(relevantCurrencies.time_last_update_utc);
            _context.CurrenciesRates.Add(currencyRates);
            _context.SaveChanges();
        }

        //Convert currecies according to USD
        private static double ConvertCurrency(double from,double to, double USD = 1)
        {
            return (USD / from) / (USD / to); 
        }

        //Getting the currecies rates from API
        private static async Task<string> GetURI(Uri u)
        {
            var response = string.Empty;
            using (var client = new HttpClient())
            {
                HttpResponseMessage result = await client.GetAsync(u);
                if (result.IsSuccessStatusCode)
                {
                    response = await result.Content.ReadAsStringAsync();
                }
            }
            return response;
        }
    }
}
