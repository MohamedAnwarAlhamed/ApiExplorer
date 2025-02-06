using ApiExplorer.Models;

namespace ApiExplorer.Data.Repositories
{
    public interface IRestaurantRepository : IRepository<Restaurant>
    {
        Task<IEnumerable<Restaurant>> GetRestaurantsByNameAsync(string name);
        Task<IEnumerable<dynamic>> GetRestaurantsByAddressAsync(string address);
        Task<IEnumerable<dynamic>> GetAllAsyncDynamic();
        Task<IEnumerable<dynamic>> GetAllFromStoredProcedureAsync();
        Task<int> GetRestaurantCountAsync();
        Task<Restaurant?> GetByIdAsync(int? id);
        Task<int> AddAsync(Restaurant entity);
        Task<int> UpdateAsync(Restaurant entity);
        Task<int> DeleteAsync(int id);
        Task<dynamic> AddAsyncByProcedure(Restaurant restaurant);
        Task<dynamic?> GetRestaurantsNameByIdAsync(int? id);


    }
}
