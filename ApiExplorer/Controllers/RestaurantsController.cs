using Microsoft.AspNetCore.Mvc;
using ApiExplorer.Data;
using ApiExplorer.Models;
using System.Threading.Tasks;

namespace ApiExplorer.Controllers
{
    [ApiController]
    [Route("api/Restaurant/[controller]")]
    public class RestaurantsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public RestaurantsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var restaurants = await _unitOfWork.RestaurantRepository.GetAllAsync();
                //var restaurants = await _unitOfWork.RestaurantRepository.GetAllAsyncDynamic();
                //var restaurants = await _unitOfWork.RestaurantRepository.GetAllFromStoredProcedureAsync();

                if (restaurants == null)
                {
                    return NotFound(new { message = "لم يتم العثور على اي مطعم" });
                }
                return Ok(restaurants);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "حدث خطأ أثناء جلب المطاعم");

                // إرجاع رسالة خطأ مخصصة
                var errorResponse = new
                {
                    message = "حدث خطأ أثناء جلب البيانات",
                    error = ex.Message,
                    details = ex.StackTrace // (اختياري) إرجاع تفاصيل إضافية
                };

                return StatusCode(500, errorResponse);
                //return StatusCode(500, new { message = "حدث خطأ أثناء جلب البيانات", error = ex.Message });
            }
        }

        [HttpGet("Aggregations")]
        public async Task<IActionResult> GetAggregations()
        {
            try
            {
                var restaurants = await _unitOfWork.RestaurantRepository.GetRestaurantCountAsync();
                //var restaurants = await _unitOfWork.RestaurantRepository.GetAllAsyncDynamic();
                //var restaurants = await _unitOfWork.RestaurantRepository.GetAllFromStoredProcedureAsync();

                if (restaurants == 0)
                {
                    return NotFound(new { message = "لا يوجد اي مطعم" });
                }
                return Ok(restaurants);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "حدث خطأ أثناء جلب المطاعم");

                // إرجاع رسالة خطأ مخصصة
                var errorResponse = new
                {
                    message = "حدث خطأ أثناء جلب البيانات",
                    error = ex.Message,
                    details = ex.StackTrace // (اختياري) إرجاع تفاصيل إضافية
                };

                return StatusCode(500, errorResponse);
                //return StatusCode(500, new { message = "حدث خطأ أثناء جلب البيانات", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            //var restaurant = await _unitOfWork.RestaurantRepository.GetByIdAsync(id);
            var restaurantName = await _unitOfWork.RestaurantRepository.GetRestaurantsNameByIdAsync(id);

            if (restaurantName == null)
            {
                return NotFound();
            }
            return Ok(restaurantName);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchByName(string name)
        {
            var restaurants = await _unitOfWork.RestaurantRepository.GetRestaurantsByNameAsync(name);
            return Ok(restaurants);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> FilterByAddress(string address)
        {
            var restaurants = await _unitOfWork.RestaurantRepository.GetRestaurantsByAddressAsync(address);
            return Ok(restaurants);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Restaurant restaurant)
        {
            restaurant.CreatedAt = DateTime.UtcNow;
            int NewId = await _unitOfWork.RestaurantRepository.AddAsync(restaurant);
            _unitOfWork.Commit();
            restaurant.RestaurantId = NewId;
            return CreatedAtAction(nameof(GetById), new { id = restaurant.RestaurantId }, restaurant);
        }

        [HttpPost("Procedure")]
        public async Task<IActionResult> CreateByProcedure(Restaurant restaurant)
        {
            restaurant.CreatedAt = DateTime.UtcNow;
            var Message = await _unitOfWork.RestaurantRepository.AddAsyncByProcedure(restaurant);
            _unitOfWork.Commit();
            return Ok(Message);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Restaurant restaurant)
        {
            if (id != restaurant.RestaurantId)
            {
                return BadRequest();
            }

            int rowsAffectedawait = await _unitOfWork.RestaurantRepository.UpdateAsync(restaurant);
            if(rowsAffectedawait == 0)
            {
                return NotFound();
            }
            _unitOfWork.Commit();
            return Ok();
        }

       

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int rowsAffected = await _unitOfWork.RestaurantRepository.DeleteAsync(id);
            _unitOfWork.Commit();
            if (rowsAffected == 0)
            {
                return NotFound();
            }
            return Ok();
        }

        
    }
}