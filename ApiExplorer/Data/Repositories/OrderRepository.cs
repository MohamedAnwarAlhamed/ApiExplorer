using ApiExplorer.Models;
using Dapper;
using System.Data;

namespace ApiExplorer.Data.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<dynamic>> GetRestaurantsWithOrdersAndUsersAsync();
        Task<IEnumerable<dynamic>> GetRestaurantsWithUsersAndOrdersAsync();
        Task<IEnumerable<dynamic>> GetRestaurantsWithUsersAndOrdersAsync2();

    }
    public class OrderRepository : IOrderRepository
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        public OrderRepository(IDbConnection connection, IDbTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public async Task<IEnumerable<dynamic>> GetRestaurantsWithOrdersAndUsersAsync()
        {
            var sql = @"
        SELECT 
            R.RestaurantId,
            R.Name,
            R.Address,
            R.Phone,
            R.CreatedAt,
            O.OrderId,
            O.OrderDate,
            O.TotalAmount,
            O.Status,
            U.UserId,
            U.Username,
            U.PasswordHash,
            U.Email,
            U.CreatedAt
            
        FROM Orders O
        INNER JOIN Users U ON U.UserId = O.UserId
        INNER JOIN Restaurants R ON R.RestaurantId = O.RestaurantId";

            var result = await _connection.QueryAsync<dynamic, dynamic, dynamic, dynamic>(
                sql,
                (restaurant, order, user) => 
                {
                    // تعيين المستخدم للطلب
                    if (order.Users == null)
                    {
                        order.Users = new List<dynamic>();
                    }
                    if (order != null)
                    {
                        order.Users.Add(user);
                    }
                    // إضافة الطلب إلى المطعم
                    if (restaurant.Orders == null)
                    {
                        restaurant.Orders = new List<dynamic>();
                    }
                    if (order != null)
                    {
                        restaurant.Orders.Add(order);
                    }
                    
                    return restaurant;
                },
                splitOn: "OrderId,UserId", // الأعمدة التي تفصل بين الكائنات
                transaction: _transaction);

            // تجميع البيانات حسب المطعم
            var groupedResult = result
                .GroupBy(r => r.RestaurantId)
                .Select(g =>
                {
                    var restaurant = g.First();
                    restaurant.Orders = g.SelectMany(r => (IEnumerable<dynamic>)r.Orders).ToList();
                    return restaurant;
                });

            return groupedResult;
        }


        public async Task<IEnumerable<dynamic>> GetRestaurantsWithUsersAndOrdersAsync()
        {
            var sql = @"
    SELECT 
        R.RestaurantId,
        R.Name AS RestaurantName,
        R.Address,
        R.Phone,
        R.CreatedAt AS RestaurantCreatedAt,
        U.UserId,
        U.Username,
        U.PasswordHash,
        U.Email,
        U.CreatedAt AS UserCreatedAt,
        O.OrderId,
        O.OrderDate,
        O.TotalAmount,
        O.Status   
    FROM Orders O
    INNER JOIN Users U ON U.UserId = O.UserId
    INNER JOIN Restaurants R ON R.RestaurantId = O.RestaurantId";

            var result = await _connection.QueryAsync<dynamic>(sql, transaction: _transaction);

            // التجميع الأول: تجميع الطلبات تحت المستخدمين
            var usersWithOrders = result
                .GroupBy(row => new
                {
                    UserId = (int)row.UserId,
                    Username = (string)row.Username,
                    PasswordHash = (string)row.PasswordHash,
                    Email = (string)row.Email,
                    UserCreatedAt = (DateTime)row.UserCreatedAt
                })
                .Select(userGroup => new
                {
                    UserId = userGroup.Key.UserId,
                    Username = userGroup.Key.Username,
                    PasswordHash = userGroup.Key.PasswordHash,
                    Email = userGroup.Key.Email,
                    CreatedAt = userGroup.Key.UserCreatedAt,
                    Orders = userGroup
                        .Select(row => new
                        {
                            OrderId = (int)row.OrderId,
                            OrderDate = (DateTime)row.OrderDate,
                            TotalAmount = (decimal)row.TotalAmount,
                            Status = (string)row.Status
                        })
                        .ToList()
                })
                .ToList();

            // التجميع الثاني: تجميع المستخدمين تحت المطاعم
            var restaurantsWithUsersAndOrders = result
                .GroupBy(row => new
                {
                    RestaurantId = (int)row.RestaurantId,
                    Name = (string)row.RestaurantName,
                    Address = (string)row.Address,
                    Phone = (string)row.Phone,
                    RestaurantCreatedAt = (DateTime)row.RestaurantCreatedAt
                })
                .Select(restaurantGroup => new
                {
                    RestaurantId = restaurantGroup.Key.RestaurantId,
                    Name = restaurantGroup.Key.Name,
                    Address = restaurantGroup.Key.Address,
                    Phone = restaurantGroup.Key.Phone,
                    CreatedAt = restaurantGroup.Key.RestaurantCreatedAt,
                    Users = usersWithOrders
                        .Where(user => restaurantGroup.Any(row => row.UserId == user.UserId))
                        .ToList()
                })
                .ToList();

            return restaurantsWithUsersAndOrders;
        }

        public async Task<IEnumerable<dynamic>> GetRestaurantsWithUsersAndOrdersAsync2()
        {
            var sql = @"
    SELECT 
        R.RestaurantId,
        R.Name AS RestaurantName,
        R.Address,
        R.Phone,
        R.CreatedAt AS RestaurantCreatedAt,
        U.UserId,
        U.Username,
        U.PasswordHash,
        U.Email,
        U.CreatedAt AS UserCreatedAt,
        O.OrderId,
        O.OrderDate,
        O.TotalAmount,
        O.Status   
    FROM Orders O
    INNER JOIN Users U ON U.UserId = O.UserId
    INNER JOIN Restaurants R ON R.RestaurantId = O.RestaurantId";

            var result = await _connection.QueryAsync<dynamic>(sql, transaction: _transaction);

            // استخدام Dictionary لتتبع المطاعم والمستخدمين
            var restaurantsDict = new Dictionary<int, dynamic>();

            foreach (var row in result)
            {
                int restaurantId = row.RestaurantId;
                int userId = row.UserId;

                // إنشاء أو تحديث المطعم
                if (!restaurantsDict.ContainsKey(restaurantId))
                {
                    restaurantsDict[restaurantId] = new
                    {
                        RestaurantId = row.RestaurantId,
                        Name = row.RestaurantName,
                        Address = row.Address,
                        Phone = row.Phone,
                        CreatedAt = row.RestaurantCreatedAt,
                        Users = new Dictionary<int, dynamic>()
                    };
                }

                var restaurant = restaurantsDict[restaurantId];

                // إنشاء أو تحديث المستخدم
                if (!restaurant.Users.ContainsKey(userId))
                {
                    restaurant.Users[userId] = new
                    {
                        UserId = row.UserId,
                        Username = row.Username,
                        PasswordHash = row.PasswordHash,
                        Email = row.Email,
                        CreatedAt = row.UserCreatedAt,
                        Orders = new List<dynamic>()
                    };
                }

                var user = restaurant.Users[userId];

                // إضافة الطلب إلى المستخدم
                user.Orders.Add(new
                {
                    OrderId = row.OrderId,
                    OrderDate = row.OrderDate,
                    TotalAmount = row.TotalAmount,
                    Status = row.Status
                });
            }

            // تحويل Dictionary إلى قائمة
            var groupedResult = restaurantsDict.Values
                .Select(r => new
                {
                    r.RestaurantId,
                    r.Name,
                    r.Address,
                    r.Phone,
                    r.CreatedAt,
                    Users = r.Users.Values
                })
                .ToList();

            return groupedResult;
        }
    }
}
