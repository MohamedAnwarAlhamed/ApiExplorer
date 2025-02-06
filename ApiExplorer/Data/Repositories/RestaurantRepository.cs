using System.Data;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using ApiExplorer.Models;
using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ApiExplorer.Data.Repositories
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        public RestaurantRepository(IDbConnection connection, IDbTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }
        //==========================================================================================
        //طرق استدعاء البيانات ككل
        //==========================================================================================
        public async Task<IEnumerable<Restaurant>> GetAllAsync()
        {
        //return null;
        //buffered: false, // تعطيل التخزين المؤقت
        //commandTimeout: 60, // مهلة 60 ثانية
            return await _connection.QueryAsync<Restaurant>("SELECT * FROM Restaurants", transaction: _transaction);
        }

        public async Task<IEnumerable<dynamic>> GetAllAsyncDynamic()
        {
            return await _connection.QueryAsync("SELECT * FROM Restaurants", transaction: _transaction);
        }

        public async Task<IEnumerable<dynamic>> GetAllFromStoredProcedureAsync()
        {
            return await _connection.QueryAsync(
        "GetAllRestaurants", // اسم الإجراء المخزن
        commandType: CommandType.StoredProcedure,
        transaction: _transaction);
        }
        //==========================================================================================
        //طرق إرجاع قيمة واحدة
        //==========================================================================================
        public async Task<int> GetRestaurantCountAsync()
        {
            return await _connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Restaurants",
                transaction: _transaction);
        }

        //==========================================================================================
        //طرق إرجاع البيانات بناء على قيمة
        //==========================================================================================
        public async Task<Restaurant?> GetByIdAsync(int? id)
        {
            //return await _connection.QueryFirstOrDefaultAsync<Restaurants>(
            //    "SELECT * FROM Restaurants WHERE RestaurantId = @Id", new { Id = id }, transaction: _transaction);
            var sql = "SELECT * FROM dbo.GetRestarantById(@RestaurantId)";
            return await _connection.QueryFirstOrDefaultAsync<Restaurant>(
                sql,
                new { RestaurantId = id },
                transaction: _transaction);
        }
        public async Task<IEnumerable<Restaurant>> GetRestaurantsByNameAsync(string name)
        {
            return await _connection.QueryAsync<Restaurant>(
                "SELECT * FROM Restaurants WHERE Name LIKE @Name",
                new { Name = $"%{name}%" },
                transaction: _transaction);
        }

        public async Task<dynamic?> GetRestaurantsNameByIdAsync(int? id)
        {
            var sql = "SELECT dbo.GetRestarantNameById(@RestaurantId)";
            return await _connection.ExecuteScalarAsync<dynamic>(
                sql,
                new { RestaurantId = id },
                transaction: _transaction);
        }

        public async Task<IEnumerable<dynamic>> GetRestaurantsByAddressAsync(string address)
        {
            return await _connection.QueryAsync(
                "SELECT * FROM Restaurants WHERE Address = @Address",
                new { Address = address },
                transaction: _transaction);
        }
        //==========================================================================================
        //طرق التعامل مع البيانات  إضافة, تعديل, حذف
        //==========================================================================================
        public async Task<int> AddAsync(Restaurant restaurant)
        {
            var sql = "INSERT INTO Restaurants (Name, Address, Phone, CreatedAt) VALUES (@Name, @Address, @Phone, @CreatedAt)";
            //await _connection.ExecuteAsync(sql, restaurant, transaction: _transaction);
            //ExecuteScalarAsync<int>, ExecuteAsync
            int NewId = await _connection.ExecuteScalarAsync<int>(sql,
                new
                {
                    Name = restaurant.Name,
                    Address = restaurant.Address,
                    Phone = restaurant.Phone,
                    CreatedAt = restaurant.CreatedAt
                }, 
                transaction: _transaction);
            return NewId;
        }

        public async Task<dynamic> AddAsyncByProcedure(Restaurant restaurant)
        {
            //var parameters = new DynamicParameters();
            //parameters.Add("RestaurantId", restaurant.Id);
            //parameters.Add("Name", restaurant.Name);
            //parameters.Add("Address", restaurant.Address);
            //parameters.Add("Phone", restaurant.Phone);
            //parameters.Add("CreatedAt", restaurant.CreatedAt);

            //var parameters = new DynamicParameters();
            //parameters.Add("RestaurantId", restaurant.Id);
            //parameters.Add("Name", restaurant.Name);
            //parameters.Add("Address", restaurant.Address);
            //parameters.Add("Phone", restaurant.Phone);
            //parameters.Add("CreatedAt", restaurant.CreatedAt);
            //parameters.Add("NewId", dbType: DbType.Int32, direction: ParameterDirection.Output); // OUTPUT parameter

            //return parameters.Get<int>("NewId");
            var parameters = new
            {
                RestaurantId = restaurant.RestaurantId,
                Name = restaurant.Name,
                Address = restaurant.Address,
                Phone = restaurant.Phone,
                CreatedAt = restaurant.CreatedAt
            };

            var sql = "InsertToRestaurants"; // اسم الإجراء المخزن
            return await _connection.QuerySingleAsync(
                sql,
                parameters,
                commandType: CommandType.StoredProcedure,
                transaction: _transaction);
        }

        public async Task<int> UpdateAsync(Restaurant restaurant)
        {
            var sql = "UPDATE Restaurants SET Name = @Name, Address = @Address, Phone = @Phone WHERE RestaurantId = @Id";
            int rowsAffected = await _connection.ExecuteAsync(sql,
                     new
                     {
                         Name = restaurant.Name,
                         Address = restaurant.Address,
                         Phone = restaurant.Phone,
                         Id = restaurant.RestaurantId
                     }
                , transaction: _transaction);
            return rowsAffected;
        }

        public async Task<int> DeleteAsync(int id)
        {
            int rowsAffected = await _connection.ExecuteAsync("DELETE FROM Restaurants WHERE RestaurantId = @Id",
                new { Id = id }, 
                transaction: _transaction);
            return rowsAffected;
        }

        
    }
}