using ApiExplorer.Data;
using ApiExplorer.Data.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiExplorer.Controllers
{
    [Route("api/MenuItem/[controller]")]
    [ApiController]
    public class MenuItemController : ControllerBase
    {

        private readonly IUnitOfWork _unitOfWork;
        public MenuItemController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("restaurants-with-menus")]
        public async Task<IActionResult> GetAllRestaurantsWithMenuItems()
        {
            var restaurantsWithMenuItems = await _unitOfWork.MenuItemRepository.GetAllRestaurantsWithMenuItemsAsync();
            return Ok(restaurantsWithMenuItems);
        }
    }
}
