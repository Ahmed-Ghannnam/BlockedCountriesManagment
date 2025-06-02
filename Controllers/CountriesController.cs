using BlockedCountries.BL.Dtos;
using BlockedCountries.BL.Managers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlockedCountries.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly BlockedCountryService _blockedCountryService;

        public CountriesController(BlockedCountryService blockedCountryService)
        {
            _blockedCountryService = blockedCountryService;
        }

        [HttpPost("block")]
        public IActionResult BlockCountry(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
                return BadRequest("Country code is required");

            if (!_blockedCountryService.AddCountry(countryCode))
                return BadRequest($"Country '{countryCode}' is already blocked");

            return Ok($"Country '{countryCode}' blocked");
        }

        [HttpDelete("block/{countryCode}")]
        public IActionResult UnblockCountry(string countryCode)
        {
            if (!_blockedCountryService.Remove(countryCode))
                return NotFound("Country not found.");

            return Ok($"Unblocked country: {countryCode}");
        }

        [HttpGet("blocked")]
        public IActionResult GetBlocked([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = _blockedCountryService.GetPaged(page, pageSize, search);
            return Ok(result);
        }
    }
}
