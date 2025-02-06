using ApiExplorer.Models;
using Dapper;
using System.Data;

namespace ApiExplorer.Data.Repositories
{
    public interface IMenuItemRepository
    {
        Task<IEnumerable<dynamic>> GetAllRestaurantsWithMenuItemsAsync();
    }

    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        public MenuItemRepository(IDbConnection connection, IDbTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public async Task<IEnumerable<dynamic>> GetAllRestaurantsWithMenuItemsAsync()
        {
            var sql = @"
        SELECT 
            r.RestaurantId, 
            r.Name AS RestaurantName, 
            r.Address, 
            r.Phone, 
            m.MenuItemId, 
            m.Name, 
            m.Description, 
            m.Price, 
            m.CreatedAt
        FROM Restaurants r
        LEFT JOIN MenuItems m ON r.RestaurantId = m.RestaurantId
        ORDER BY r.RestaurantId";
        var result = await _connection.QueryAsync<dynamic, dynamic, dynamic>(
        sql, (restaurant, menuItem) =>
        {
            if (restaurant.MenuItems == null)
            {
                restaurant.MenuItems = new List<dynamic>();
            }
            if (menuItem != null)
            {
                restaurant.MenuItems.Add(menuItem);
            }
            return restaurant;
        },
        splitOn: "MenuItemId", // العمود الذي يفصل بين الكائنات
        transaction: _transaction);

        // تجميع البيانات حسب المطعم
        var groupedResult = result
            .GroupBy(r => r.RestaurantId) // IGrouping<int, dynamic>
            .Select(g =>
            {
                var restaurant = g.First();
                restaurant.MenuItems = g.SelectMany((r) => (IEnumerable<dynamic>)r.MenuItems).ToList();
                return restaurant;
            });

            return groupedResult;

        }
    }
}
