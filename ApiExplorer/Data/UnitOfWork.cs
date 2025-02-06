using ApiExplorer.Data.Repositories;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiExplorer.Data
{
    public interface IUnitOfWork : IDisposable
    {
        void Commit();
        void Rollback();
        IRepository<T> GetRepository<T>() where T : class;
        IRestaurantRepository RestaurantRepository { get; }
        IMenuItemRepository MenuItemRepository { get; }
        IOrderRepository orderRepository { get; }

    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbConnection _connection;
        private IDbTransaction _transaction;
        private IRestaurantRepository? _restaurantRepository;
        private IMenuItemRepository? _menuItemRepository;
        private IOrderRepository? _orderRepository;

        public UnitOfWork(IConfiguration configuration)
        {
            _connection = new SqlConnection(configuration.GetConnectionString("ConnectionDB"));
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            return new Repository<T>(_connection, _transaction);
        }

        public IRestaurantRepository RestaurantRepository
        {
            get
            {
                if (_restaurantRepository == null)
                {
                    _restaurantRepository = new RestaurantRepository(_connection, _transaction);
                }
                return _restaurantRepository;
            }
        }

        public IMenuItemRepository MenuItemRepository
        {
            get
            {
                if (_menuItemRepository == null)
                {
                    _menuItemRepository = new MenuItemRepository(_connection, _transaction);
                }
                return _menuItemRepository;
            }
        }

        public IOrderRepository orderRepository
        {
            get
            {
                if (_orderRepository == null)
                {
                    _orderRepository = new OrderRepository(_connection, _transaction);
                }
                return _orderRepository;
            }
        }

        public void Commit()
        {
            try
            {
                _transaction.Commit();
            }
            catch
            {
                _transaction.Rollback();
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = _connection.BeginTransaction();
            }
        }

        public void Rollback()
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = _connection.BeginTransaction();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }
    }
}
