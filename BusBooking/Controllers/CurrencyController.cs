using BusBooking.Services.ServiceManager;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Controllers
{
    [ApiController]
    [Route("api/currency")]
    public class CurrencyController(IServiceManager services) : ControllerBase
    {
        private readonly IServiceManager _services = services;

        // GET api/currency/rates — public, prices are public info. Rates are
        // EUR-based (RouteInfo.Price/Trip.Price are stored in EUR) and cached
        // server-side for a few hours (see CurrencyRateService), so this is
        // cheap to call on every page load.
        [HttpGet("rates")]
        public async Task<IActionResult> GetRates(CancellationToken cancellationToken)
        {
            var rates = await _services.CurrencyRateService.GetRatesAsync(cancellationToken);
            return Ok(new { @base = rates.Base, rates = rates.Rates, asOf = rates.AsOf });
        }
    }
}
