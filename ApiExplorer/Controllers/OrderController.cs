using ApiExplorer.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ApiExplorer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("restaurants-with-orders-with-users")]
        public async Task<IActionResult> GetRestaurantsWithOrdersAndUsers()
        {
            //var restaurantsWithOrdersAndUsers = await _unitOfWork.orderRepository.GetRestaurantsWithOrdersAndUsersAsync();
            //var restaurantsWithUsersAndOrdersAsync = await _unitOfWork.orderRepository.GetRestaurantsWithUsersAndOrdersAsync();
            var restaurantsWithUsersAndOrdersAsync = await _unitOfWork.orderRepository.GetRestaurantsWithUsersAndOrdersAsync2();

            return Ok(restaurantsWithUsersAndOrdersAsync);
        }


        [HttpGet("via-header")]
        public IActionResult GetViaHeader([FromHeader(Name = "Custom-Header")] string customHeaderValue)
        {
            if (string.IsNullOrEmpty(customHeaderValue))
            {
                return BadRequest("Custom-Header is required.");
            }

            return Ok($"Received value from header: {customHeaderValue}");
        }

        [HttpGet("via-query")]
        public IActionResult GetViaQuery([FromQuery] string paramValue)
        {
            if (string.IsNullOrEmpty(paramValue))
            {
                return BadRequest("paramValue is required.");
            }

            return Ok($"Received value from query string: {paramValue}");
        }

        [HttpGet("via-route/{paramValue}")]
        public IActionResult GetViaRoute([FromRoute] string paramValue)
        {
            if (string.IsNullOrEmpty(paramValue))
            {
                return BadRequest("paramValue is required.");
            }

            return Ok($"Received value from route: {paramValue}");
        }

        [HttpPost("via-header-query-or-body")]
        public IActionResult PostViaHeaderQueryOrBody(
    [FromHeader(Name = "Custom-Header")] string customHeaderValue,
    [FromQuery] string paramValue,
    [FromBody] dynamic model)
        {
            if (string.IsNullOrEmpty(customHeaderValue) && string.IsNullOrEmpty(paramValue) && model == null)
            {
                return BadRequest("Either Custom-Header, paramValue, or model is required.");
            }

            var receivedValue = !string.IsNullOrEmpty(customHeaderValue) ? customHeaderValue :
                                !string.IsNullOrEmpty(paramValue) ? paramValue :
                                JsonSerializer.Serialize(model);

            return Ok($"Received value: {receivedValue}");
        }
    }
}
