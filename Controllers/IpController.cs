using BlockedCountries.BL.Dtos;
using BlockedCountries.BL.Managers;
using BlockedCountries.BL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BlockedCountries.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IpController : ControllerBase
    {
        private readonly GeoLocationService _geoService;
        private readonly BlockedCountryService _blockedService;
        private readonly LogService _logService;

        public IpController(GeoLocationService geoService, BlockedCountryService blockedService, LogService logService)
        {
            _geoService = geoService;
            _blockedService = blockedService;
            _logService = logService;
        }
        [HttpGet("lookup")]
        public async Task<IActionResult> Lookup([FromQuery] string? ipAddress = null)
        {
            try
            {
                ipAddress ??= HttpContext.Connection.RemoteIpAddress?.ToString();

                if (string.IsNullOrWhiteSpace(ipAddress) || !IPAddress.TryParse(ipAddress, out _))
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse("Invalid or missing IP address."));
                }

                var result = await _geoService.GetFullIpInfoAsync(ipAddress);

                if (result == null)
                {
                    return NotFound(ApiResponseDto<object>.ErrorResponse("IP information not found."));
                }

                return Ok(ApiResponseDto<object>.SuccessResponse(result, "IP lookup successful"));
            }
            catch (Exception ex)
            {           
                return BadRequest( ApiResponseDto<object>.ErrorResponse(
                   $"{ ex.InnerException}",
                    new List<string> { ex.Message }));
            }
        }

        [HttpGet("check-block")]
        public async Task<IActionResult> CheckIfBlocked()
        {
            var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
       ?? HttpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrWhiteSpace(ip) || ip == "::1" || ip == "127.0.0.1")
            {
                ip = await _geoService.GetServerExternalIpAsync();
            }

            var result = await _geoService.GetFullIpInfoAsync(ip);
            if (result == null)
                return BadRequest("IP lookup failed.");

            var isBlocked = !string.IsNullOrWhiteSpace(result.CountryCode) &&
                            _blockedService.IsBlocked(result.CountryCode);

            _logService.AddLog(new IpBlockCheckLogDto
            {
                IpAddress = ip,
                CountryCode = result.CountryCode,
                IsBlocked = isBlocked
            });

            return Ok(new
            {
                result.IpAddress,
                result.CountryCode,
                result.CountryName,
                result.ISP,
                IsBlocked = isBlocked
            });
        }
    }
}
